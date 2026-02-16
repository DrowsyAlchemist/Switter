using Microsoft.Extensions.Options;
using NotificationService.Events.Tweet;
using NotificationService.Interfaces;
using NotificationService.Interfaces.Data;
using NotificationService.Interfaces.Infrastructure;
using NotificationService.Models;
using NotificationService.Models.Options;

namespace NotificationService.Services.EventHandlers.TweetEventHandlers
{
    public class ReplyEventHandler : NotificationEventHandler
    {
        private readonly string _eventName;

        public ReplyEventHandler(
            INotificationDeliveryService deliveryService,
            IOptions<KafkaOptions> options,
            IProfileServiceClient profileService,
            INotificationEventsProcessor eventsProcessor)
            : base(eventsProcessor, deliveryService, profileService, options)
        {
            _eventName = options.Value.TweetEvents.ReplyCreatedEventName;
        }

        public override async Task HandleAsync(string eventName, string eventJson)
        {
            if (eventName != _eventName)
                return;

            var replyEvent = DeserializeEvent<ReplyEvent>(eventJson);

            var userInfo = await ProfileService.GetUserInfoAsync(replyEvent.InitiatorUserId);

            var notification = new Notification
            {
                UserId = replyEvent.ParentTweetAuthorId,
                Title = "Новый ответ",
                Message = $"Пользователь прокомментировал ваш твит",
                Type = NotificationType.Reply,
                SourceUserId = replyEvent.InitiatorUserId,
                SourceUserName = userInfo?.DisplayName,
                SourceUserAvatarUrl = userInfo?.AvatarUrl,
                SourceTweetId = replyEvent.TweetId,
                ShouldSendWebSocket = true,
                ShouldSendPush = false,
                ShouldSendEmail = false
            };
            await DeliveryService.DeliverNotificationAsync(notification);
        }
    }
}
