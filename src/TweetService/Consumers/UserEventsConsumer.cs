using System.Text.Json;
using TweetService.Events;
using TweetService.Interfaces.Data.Repositories;

namespace TweetService.Consumers
{
    public class UserEventsConsumer : EventsConsumer
    {
        public UserEventsConsumer(IConfiguration configuration, IServiceProvider serviceProvider,  ILogger<UserEventsConsumer> logger)
            : base(configuration, serviceProvider, logger)
        {
        }

        protected override IEnumerable<string> GetTopics()
        {
            return ["user-profile-changed", "user-profile-deleted"];
        }

        protected override async Task ProcessMessageAsync(string topic, string message, CancellationToken cancellationToken)
        {
            using var scope = ServiceProvider.CreateScope();

            try
            {
                switch (topic)
                {
                    case "user-profile-changed":
                        await HandleUserProfileChanged(message, scope.ServiceProvider, cancellationToken);
                        break;

                    case "user-profile-deleted":
                        await HandleUserProfileDeleted(message, scope.ServiceProvider, cancellationToken);
                        break;

                    default:
                        Logger.LogWarning("Unknown topic: {Topic}", topic);
                        break;
                }
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
                throw new InvalidOperationException("Failed to deserialize user profile changed event");

            Logger.LogInformation("Processing user profile update for user {UserId}", userEvent.UserId);

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
                throw new InvalidOperationException("Failed to deserialize user profile changed event");

            Logger.LogInformation("Processing user profile update for user {UserId}", userEvent.UserId);

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