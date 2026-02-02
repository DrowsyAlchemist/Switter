using Microsoft.Extensions.Options;
using System.Text.Json;
using TweetService.Events;
using TweetService.Interfaces.Infrastructure;
using TweetService.Models.Options;

namespace TweetService.Consumers
{
    public class TweetEventsConsumer : EventsConsumer
    {
        private readonly string _likeDeletedTopic;
        private readonly string _keyForTrendTweets;
        private readonly string _keyForLastLikedTweets;
        private readonly IServiceProvider _serviceProvider;

        public TweetEventsConsumer(
            IServiceProvider serviceProvider,
            IOptions<KafkaOptions> kafkaOptions,
            IOptions<TrendsOptions> trendsOptions,
            ILogger<TweetEventsConsumer> logger)
           : base(kafkaOptions, logger)
        {
            _serviceProvider = serviceProvider;
            _likeDeletedTopic = kafkaOptions.Value.TweetEvents.TweetDeletedEventName;
            _keyForTrendTweets = trendsOptions.Value.Cache.KeyForTrendTweets;
            _keyForLastLikedTweets = trendsOptions.Value.KeyForLastLikedTweets;
        }

        protected override IEnumerable<string> GetTopics()
        {
            return [_likeDeletedTopic];
        }

        protected override async Task ProcessMessageAsync(string topic, string message, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            try
            {
                if (topic == _likeDeletedTopic)
                    await HandleLikeDeletedAsync(message, scope.ServiceProvider);
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

        private async Task HandleLikeDeletedAsync(string message, IServiceProvider services)
        {
            var likeEvent = JsonSerializer.Deserialize<LikeSetEvent>(message);

            if (likeEvent == null)
                throw new JsonException($"Failed to deserialize {_likeDeletedTopic} event");

            Logger.LogInformation("Processing {topic} for user {UserId}", _likeDeletedTopic, likeEvent.UserId);

            var redis = services.GetRequiredService<IRedisService>();
            await redis.RemoveKeyAsync(_keyForLastLikedTweets);
            await redis.RemoveKeyAsync(_keyForTrendTweets);
        }
    }
}
