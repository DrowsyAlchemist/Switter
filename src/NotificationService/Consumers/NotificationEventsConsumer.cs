using Microsoft.Extensions.Options;
using NotificationService.Interfaces;
using NotificationService.Models.Options;
using System.Text.Json;

namespace NotificationService.Consumers
{
    public class NotificationEventsConsumer : EventsConsumer
    {
        private readonly INotificationEventsProcessor _eventsProcessor;
        private readonly KafkaOptions _options;
        private readonly ILogger<NotificationEventsConsumer> _logger;

        private readonly List<string> _topics;

        public NotificationEventsConsumer(
            INotificationEventsProcessor eventsProcessor,
            IOptions<KafkaOptions> kafkaOptions,
            ILogger<NotificationEventsConsumer> logger)
            : base(kafkaOptions, logger)
        {
            _eventsProcessor = eventsProcessor;
            _options = kafkaOptions.Value;
            _logger = logger;
            _topics =
            [
                _options.UserEvents.UserFollowedEventName,
                _options.TweetEvents.TweetCreatedEventName,
                _options.TweetEvents.RetweetEventName,
                _options.TweetEvents.ReplyCreatedEventName,
                _options.TweetEvents.LikeSetEventName,
            ];
        }

        protected override IEnumerable<string> GetTopics()
        {
            return _topics;
        }

        protected override async Task ProcessMessageAsync(string topic, string message, CancellationToken cancellationToken)
        {
            try
            {
                if (_topics.Contains(topic))
                    await _eventsProcessor.ProcessEventAsync(topic, message);
                else
                    throw new NotImplementedException();
            }
            catch (JsonException ex)
            {
                Logger.LogError(ex, "Error deserializing message from topic {Topic}: {Message}",
                    topic, message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from topic {Topic}", topic);
                throw;
            }
        }
    }
}