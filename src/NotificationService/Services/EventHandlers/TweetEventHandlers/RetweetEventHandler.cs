using Microsoft.Extensions.Options;
using NotificationService.Events.Tweet;
using NotificationService.Interfaces;
using NotificationService.Interfaces.Infrastructure;
using NotificationService.Models;
using NotificationService.Models.Options;

namespace NotificationService.Services.EventHandlers.TweetEventHandlers
{
    public class RetweetEventHandler : NotificationEventHandler
    {
        private readonly string _eventName;

        public RetweetEventHandler(
            IServiceProvider serviceProvider,
            IOptions<KafkaOptions> options,
            IProfileServiceClient profileService,
            INotificationEventsProcessor eventsProcessor)
            : base(eventsProcessor, serviceProvider, profileService, options)
        {
            _eventName = options.Value.TweetEvents.RetweetEventName;
        }

        public override async Task HandleAsync(string eventName, string eventJson)
        {
            if (eventName != _eventName)
                return;

            var retweetEvent = DeserializeEvent<RetweetEvent>(eventJson);

            var userInfo = await ProfileService.GetUserInfoAsync(retweetEvent.InitiatorUserId);

            var notification = new Notification
            {
                UserId = retweetEvent.ParentTweetAuthorId,
                Title = "Новый ретвит",
                Message = $"Пользователь ретвитнул ваш твит",
                Type = NotificationType.Retweet,
                SourceUserId = retweetEvent.InitiatorUserId,
                SourceUserName = userInfo?.DisplayName,
                SourceUserAvatarUrl = userInfo?.AvatarUrl,
                SourceTweetId = retweetEvent.TweetId,
                ShouldSendWebSocket = true,
                ShouldSendPush = false,
                ShouldSendEmail = false
            };
            await DeliveryService.DeliverNotificationAsync(notification);
        }
    }
}
