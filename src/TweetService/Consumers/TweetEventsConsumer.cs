using System.Text.Json;
using TweetService.Events;
using TweetService.Interfaces.Infrastructure;

namespace TweetService.Consumers
{
    public class TweetEventsConsumer : EventsConsumer
    {
        private const string KeyForLikes = "KeyForTweetLikes";

        public TweetEventsConsumer(
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            ILogger<TweetEventsConsumer> logger)
           : base(configuration, serviceProvider, logger)
        {
        }

        protected override IEnumerable<string> GetTopics()
        {
            return ["like-set"];
        }

        protected override async Task ProcessMessageAsync(string topic, string message, CancellationToken cancellationToken)
        {
            using var scope = ServiceProvider.CreateScope();

            try
            {
                switch (topic)
                {
                    case "like-set":
                        await HandleLikeSetAsync(message, scope.ServiceProvider);
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

        private async Task HandleLikeSetAsync(string message, IServiceProvider services)
        {
            var likeEvent = JsonSerializer.Deserialize<LikeSetEvent>(message);

            if (likeEvent == null)
                throw new JsonException("Failed to deserialize like-set event");

            Logger.LogInformation("Processing like-set for user {UserId}", likeEvent.UserId);

            var redis = services.GetRequiredService<IRedisService>();
            var likedTweetId = likeEvent.TweetId.ToString();
            await redis.AddToListAsync(KeyForLikes, [likedTweetId]);
        }
    }
}
