using Microsoft.Extensions.Options;
using NotificationService.Interfaces.Infrastructure;
using NotificationService.Interfaces;
using NotificationService.Models.Options;
using NotificationService.Models;
using NotificationService.Events.User;

namespace NotificationService.Services.EventHandlers.UserEventHandlers
{
    public class FollowEventHandler : NotificationEventHandler
    {
        private readonly string _eventName;

        public FollowEventHandler(
            INotificationDeliveryService deliveryService,
            IOptions<KafkaOptions> options,
            IProfileServiceClient profileService,
            INotificationEventsProcessor eventsProcessor)
            : base(eventsProcessor, deliveryService, profileService, options)
        {
            _eventName = options.Value.UserEvents.UserFollowedEventName;
        }

        public override async Task HandleAsync(string eventName, string eventJson)
        {
            if (eventName != _eventName)
                return;

            var followEvent = DeserializeEvent<UserFollowedEvent>(eventJson);
            var userInfo = await ProfileService.GetUserInfoAsync(followEvent.FollowedUserId);

            var notification = new Notification
            {
                UserId = followEvent.FollowingUserId,
                Title = "Новый подписчик",
                Message = $"Пользователь подписался на вас",
                Type = NotificationType.Follow,
                SourceUserId = followEvent.FollowedUserId,
                SourceUserName = userInfo?.DisplayName,
                SourceUserAvatarUrl = userInfo?.AvatarUrl,
                ShouldSendWebSocket = true,
                ShouldSendPush = false,
                ShouldSendEmail = false
            };
            await DeliveryService.DeliverNotificationAsync(notification);
        }
    }
}
