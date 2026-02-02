using Microsoft.Extensions.Options;
using System.Text.Json;
using TweetService.Events;
using TweetService.Interfaces.Infrastructure;
using TweetService.Models.Options;

namespace TweetService.Consumers
{
    public class TweetEventsConsumer : EventsConsumer
    {
        private const string KeyForLikes = "KeyForTweetLikes";
        private readonly string _likeSetTopic;
        private readonly IServiceProvider _serviceProvider;

        public TweetEventsConsumer(IServiceProvider serviceProvider, IOptions<KafkaOptions> options, ILogger<TweetEventsConsumer> logger)
           : base(options, logger)
        {
            _serviceProvider = serviceProvider;
            _likeSetTopic = options.Value.TweetEvents.LikeSetEventName;
        }

        protected override IEnumerable<string> GetTopics()
        {
            return [_likeSetTopic];
        }

        protected override async Task ProcessMessageAsync(string topic, string message, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            try
            {
                if (topic == _likeSetTopic)
                    await HandleLikeSetAsync(message, scope.ServiceProvider);
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

        private async Task HandleLikeSetAsync(string message, IServiceProvider services)
        {
            var likeEvent = JsonSerializer.Deserialize<LikeSetEvent>(message);

            if (likeEvent == null)
                throw new JsonException($"Failed to deserialize {_likeSetTopic} event");

            Logger.LogInformation("Processing {topic} for user {UserId}", _likeSetTopic, likeEvent.UserId);

            var redis = services.GetRequiredService<IRedisService>();
            var likedTweetId = likeEvent.TweetId.ToString();
            await redis.AddToListAsync(KeyForLikes, [likedTweetId]);
        }
    }
}
