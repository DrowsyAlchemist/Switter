using Microsoft.Extensions.Options;
using System.Text.Json;
using TweetService.Events;
using TweetService.Interfaces.Data.Repositories;
using TweetService.Models.Options;

namespace TweetService.Consumers
{
    public class UserEventsConsumer : EventsConsumer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _profileChangedTopicName;
        private readonly string _profileDeletedTopicName;

        public UserEventsConsumer(IServiceProvider serviceProvider, IOptions<KafkaOptions> options, ILogger<UserEventsConsumer> logger)
            : base(options, logger)
        {
            _serviceProvider = serviceProvider;
            _profileChangedTopicName = options.Value.ProfileEvents.ProfileChangedEventName;
            _profileDeletedTopicName = options.Value.ProfileEvents.ProfileDeletedEventName;
        }

        protected override IEnumerable<string> GetTopics()
        {
            return [_profileChangedTopicName, _profileDeletedTopicName];
        }

        protected override async Task ProcessMessageAsync(string topic, string message, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            try
            {
                if (topic == _profileChangedTopicName)
                    await HandleUserProfileChanged(message, scope.ServiceProvider, cancellationToken);
                else if (topic == _profileDeletedTopicName)
                    await HandleUserProfileDeleted(message, scope.ServiceProvider, cancellationToken);
                else
                    Logger.LogWarning("Unknown topic: {Topic}", topic);

                Logger.LogInformation("Successfully processed message from topic {Topic}", topic);
            }
            catch (JsonException ex)
            {
                Logger.LogError(ex, "Error deserializing message from topic {Topic}: {Message}",
                    topic, message);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Logger.LogError(ex, "Error processing message from topic {Topic}", topic);
            }
        }

        private async Task HandleUserProfileChanged(string message, IServiceProvider services, CancellationToken cancellationToken)
        {
            var userEvent = JsonSerializer.Deserialize<UserProfileChangedEvent>(message);

            if (userEvent == null)
                throw new InvalidOperationException($"Failed to deserialize {_profileChangedTopicName} event");

            Logger.LogInformation("Processing {topic} for user {UserId}", _profileChangedTopicName, userEvent.UserId);

            if (userEvent.DisplayName == null && userEvent.AvatarUrl == null)
            {
                Logger.LogInformation("No changes required for user {UserId}", userEvent.UserId);
                return;
            }

            var tweetRepository = services.GetRequiredService<ITweetRepository>();
            var userTweets = await tweetRepository.GetByUserAsync(userEvent.UserId, 1, int.MaxValue);

            foreach (var tweet in userTweets)
            {
                if (userEvent.DisplayName != null)
                    tweet.AuthorDisplayName = userEvent.DisplayName;

                if (userEvent.AvatarUrl != null)
                    tweet.AuthorAvatarUrl = userEvent.AvatarUrl;
            }
            await tweetRepository.UpdateRangeAsync(userTweets);
        }

        private async Task HandleUserProfileDeleted(string message, IServiceProvider services, CancellationToken cancellationToken)
        {
            var userEvent = JsonSerializer.Deserialize<UserProfileDeletedEvent>(message);

            if (userEvent == null)
                throw new InvalidOperationException($"Failed to deserialize {_profileDeletedTopicName} event");

            Logger.LogInformation("Processing {topic} for user {UserId}", _profileDeletedTopicName, userEvent.UserId);

            var tweetRepository = services.GetRequiredService<ITweetRepository>();
            var userTweetIds = await tweetRepository.GetIdsByUserAsync(userEvent.UserId, 1, int.MaxValue);

            if (userTweetIds.Any())
            {
                await tweetRepository.SoftDeleteRangeAsync(userTweetIds);
                Logger.LogInformation("Deleted {Count} tweets for user {UserId}", userTweetIds.Count(), userEvent.UserId);
            }
            else
            {
                Logger.LogInformation("No tweets found for user {UserId}", userEvent.UserId);
            }
        }
    }
}