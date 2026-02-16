using Microsoft.Extensions.Options;
using NotificationService.Events.Tweet;
using NotificationService.Interfaces;
using NotificationService.Interfaces.Infrastructure;
using NotificationService.Models;
using NotificationService.Models.Options;

namespace NotificationService.Services.EventHandlers.TweetEventHandlers
{
    public class LikeSetEventHandler : NotificationEventHandler
    {
        private readonly string _eventName;

        public LikeSetEventHandler(
            INotificationDeliveryService deliveryService,
            IOptions<KafkaOptions> options,
            IProfileServiceClient profileService,
            INotificationEventsProcessor eventsProcessor)
            : base(eventsProcessor, deliveryService, profileService, options)
        {
            _eventName = options.Value.TweetEvents.LikeSetEventName;
        }

        public override async Task HandleAsync(string eventName, string eventJson)
        {
            if (eventName != _eventName)
                return;

            var likeEvent = DeserializeEvent<LikeSetEvent>(eventJson);
            var userInfo = await ProfileService.GetUserInfoAsync(likeEvent.InitiatorUserId);

            var notification = new Notification
            {
                UserId = likeEvent.TweetAuthorId,
                Title = "Новый лайк",
                Message = $"Пользователю понравился ваш твит",
                Type = NotificationType.Like,
                SourceUserId = likeEvent.InitiatorUserId,
                SourceUserName = userInfo?.DisplayName,
                SourceUserAvatarUrl = userInfo?.AvatarUrl,
                SourceTweetId = likeEvent.TweetId,
                ShouldSendWebSocket = true,
                ShouldSendPush = false,
                ShouldSendEmail = false
            };
            await DeliveryService.DeliverNotificationAsync(notification);
        }
    }
}
