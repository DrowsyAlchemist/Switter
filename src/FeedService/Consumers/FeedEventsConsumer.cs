using FeedService.Events;
using FeedService.Interfaces;
using FeedService.Models.Options;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FeedService.Consumers
{
    public class FeedEventsConsumer : EventsConsumer
    {
        private readonly IFeedEventProcessor _eventProcessor;

        private readonly string _tweetCreatedEventName;
        private readonly string _retweetCreatedEventName;
        private readonly string _likeSetEventName;

        private readonly string _userFollowedEventName;
        private readonly string _userUnfollowedEventName;
        private readonly string _userBlockedEventName;

        public FeedEventsConsumer(
            IFeedEventProcessor eventProcessor,
            IOptions<KafkaOptions> kafkaOptions,
            ILogger<FeedEventsConsumer> logger)
           : base(kafkaOptions, logger)
        {
            _eventProcessor = eventProcessor;
            var options = kafkaOptions.Value;

            _tweetCreatedEventName = options.TweetEvents.TweetCreatedEventName;
            _retweetCreatedEventName = options.TweetEvents.RetweetCreatedEventName;
            _likeSetEventName = options.TweetEvents.LikeSetEventName;

            _userFollowedEventName = options.ProfileEvents.UserFollowedEventName;
            _userUnfollowedEventName = options.ProfileEvents.UserUnfollowedEventName;
            _userBlockedEventName = options.ProfileEvents.UserBlockedEventName;
        }

        protected override IEnumerable<string> GetTopics()
        {
            return
                [
                    _tweetCreatedEventName,
                    _retweetCreatedEventName,
                    _likeSetEventName,

                    _userFollowedEventName,
                    _userUnfollowedEventName,
                    _userBlockedEventName
                ];
        }

        protected override async Task ProcessMessageAsync(string topic, string message, CancellationToken cancellationToken)
        {
            try
            {
                await HandleMessage(topic, message);
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

        private async Task HandleMessage(string topic, string message)
        {
            if (topic == _tweetCreatedEventName)
            {
                var @event = DeserializeEvent<TweetCreatedEvent>(message);
                await _eventProcessor.ProcessTweetCreatedAsync(@event);
            }
            else if (topic == _retweetCreatedEventName)
            {
                var @event = DeserializeEvent<RetweetCreatedEvent>(message);
                await _eventProcessor.ProcessRetweetAsync(@event);
            }
            else if (topic == _likeSetEventName)
            {
                var @event = DeserializeEvent<LikeSetEvent>(message);
                await _eventProcessor.ProcessTweetLikedAsync(@event);
            }
            else if (topic == _userFollowedEventName)
            {
                var @event = DeserializeEvent<UserFollowedEvent>(message);
                await _eventProcessor.ProcessUserFollowedAsync(@event);
            }
            else if (topic == _userUnfollowedEventName)
            {
                var @event = DeserializeEvent<UserUnfollowedEvent>(message);
                await _eventProcessor.ProcessUserUnfollowedAsync(@event);
            }
            else if (topic == _userBlockedEventName)
            {
                var @event = DeserializeEvent<UserBlockedEvent>(message);
                await _eventProcessor.ProcessUserBlockedAsync(@event);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private T DeserializeEvent<T>(string message)
        {
            var @event = JsonSerializer.Deserialize<T>(message);

            if (@event == null)
                throw new JsonException($"Failed to deserialize {nameof(T)} event");

            return @event;
        }
    }
}
