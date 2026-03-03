using Microsoft.Extensions.Options;
using NotificationService.Events.Tweet;
using NotificationService.Interfaces;
using NotificationService.Interfaces.Infrastructure;
using NotificationService.Models;
using NotificationService.Models.Options;

namespace NotificationService.Services.EventHandlers.TweetEventHandlers
{
    public class TweetCreatedEventHandler : NotificationEventHandler
    {
        private readonly string _eventName;

        public TweetCreatedEventHandler(
            IServiceProvider serviceProvider,
            IOptions<KafkaOptions> options,
            IProfileServiceClient profileService,
            INotificationEventsProcessor eventsProcessor)
            : base(eventsProcessor, serviceProvider, profileService, options)
        {
            _eventName = options.Value.TweetEvents.TweetCreatedEventName;
        }

        public override async Task HandleAsync(string eventName, string eventJson)
        {
            if (eventName != _eventName)
                return;

            var tweetEvent = DeserializeEvent<TweetCreatedEvent>(eventJson);

            var authorInfo = await ProfileService.GetUserInfoAsync(tweetEvent.InitiatorUserId);

            var followers = await ProfileService.GetFollowersIds(tweetEvent.InitiatorUserId);

            foreach (var follower in followers)
            {
                var notification = new Notification
                {
                    UserId = follower,
                    Title = "Новый твит",
                    Message = $"Этот твит может быть вам интересен",
                    Type = NotificationType.Tweet,
                    SourceUserId = tweetEvent.InitiatorUserId,
                    SourceUserName = authorInfo?.DisplayName,
                    SourceUserAvatarUrl = authorInfo?.AvatarUrl,
                    SourceTweetId = tweetEvent.TweetId,
                    ShouldSendWebSocket = true,
                    ShouldSendPush = false,
                    ShouldSendEmail = false
                };
                await DeliveryService.DeliverNotificationAsync(notification);
            }
        }
    }
}