using AutoMapper;
using NotificationService.DTOs;
using NotificationService.Interfaces;
using NotificationService.Interfaces.Data;
using NotificationService.Models;

namespace NotificationService.Services
{
    public class NotificationDeliveryService : INotificationDeliveryService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationSettingsService _notificationSettingsService;
        private readonly IWebSocketMessager _webSocketMessager;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationDeliveryService> _logger;

        public NotificationDeliveryService(
            INotificationRepository notificationRepository,
            INotificationSettingsService notificationSettingsService,
            IWebSocketMessager webSocketMessager,
            IMapper mapper,
            ILogger<NotificationDeliveryService> logger)
        {
            _notificationRepository = notificationRepository;
            _notificationSettingsService = notificationSettingsService;
            _webSocketMessager = webSocketMessager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task DeliverNotificationAsync(Notification notification)
        {
            ArgumentNullException.ThrowIfNull(notification);
            try
            {
                await _notificationRepository.AddAsync(notification);

                var userSettings = await _notificationSettingsService.GetSettingsAsync(notification.UserId);

                if (ShouldSendNotification(userSettings, notification.Type) == false)
                {
                    _logger.LogDebug("Notification type {Type} disabled for user {UserId}",
                        notification.Type, notification.UserId);

                    notification.Status = NotificationStatus.Failed;
                    notification.ErrorMessage += $"Notification type disabled.\n";
                    await _notificationRepository.UpdateAsync(notification);
                    return;
                }

                bool sent = false;

                // WebSocket
                if (userSettings.EnableWebSocketNotifications && notification.ShouldSendWebSocket)
                {
                    bool result = await DeliverWebSocketNotificationAsync(notification);
                    if (result)
                    {
                        sent = true;
                        notification.SentAt = DateTime.UtcNow;
                        notification.Status = NotificationStatus.Unread;
                    }
                    else
                    {
                        notification.Status = NotificationStatus.Failed;
                        notification.ErrorMessage += $"Web socket error.\n";
                    }
                }

                // Push
                if (userSettings.EnablePushNotifications && notification.ShouldSendPush)
                {
                    bool result = await DeliverPushNotificationAsync(notification);

                    if (result == false)
                    {
                        notification.ErrorMessage += $"Push notifications error.\n";
                        if (sent == false)
                            notification.Status = NotificationStatus.Failed;
                    }
                    else if (sent == false)
                    {
                        sent = true;
                        notification.SentAt = DateTime.UtcNow;
                        notification.Status = NotificationStatus.Sent;
                    }
                }

                // Email
                if (userSettings.EnableEmailNotifications && notification.ShouldSendEmail)
                {
                    bool result = await DeliverEmailNotificationAsync(notification);

                    if (result == false)
                    {
                        notification.ErrorMessage += $"Email notifications error.\n";
                        if (sent == false)
                            notification.Status = NotificationStatus.Failed;
                    }
                    else if (sent == false)
                    {
                        sent = true;
                        notification.SentAt = DateTime.UtcNow;
                        notification.Status = NotificationStatus.Sent;
                    }
                }
                await _notificationRepository.UpdateAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deliver notification {NotificationId}", notification.Id);
            }
        }

        public async Task<bool> DeliverWebSocketNotificationAsync(Notification notification)
        {
            try
            {
                var userId = notification.UserId;
                var notificationDto = _mapper.Map<NotificationDto>(notification);
                var message = new WebSocketMessage
                {
                    Type = "notification",
                    Data = notificationDto,
                    Timestamp = DateTime.UtcNow
                };
                await _webSocketMessager.SendToUserAsync(userId, message);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task BroadcastSystemNotificationAsync(string message)
        {
            var systemNotification = new WebSocketMessage
            {
                Data = new
                {
                    Message = message,
                    Type = "system"
                },
                Timestamp = DateTime.UtcNow
            };
            await _webSocketMessager.BroadcastSystemMessageAsync(systemNotification);
        }

        private static bool ShouldSendNotification(UserNotificationSettingsDto settings, NotificationType type)
        {
            return type switch
            {
                NotificationType.Like => settings.EnableLikeNotifications,
                NotificationType.Retweet => settings.EnableRetweetNotifications,
                NotificationType.Reply => settings.EnableReplyNotifications,
                NotificationType.Follow => settings.EnableFollowNotifications,
                NotificationType.Message => settings.EnableMessageNotifications,
                NotificationType.System => settings.EnableSystemNotifications,
                _ => throw new NotImplementedException()
            };
        }

        private async Task<bool> DeliverPushNotificationAsync(Notification notification)
        {
            _logger.LogInformation("[DEMO] Push notification would be sent to user {UserId}: {Title}",
                notification.UserId, notification.Title);
            await Task.Delay(10);
            return true;
        }

        private async Task<bool> DeliverEmailNotificationAsync(Notification notification)
        {
            _logger.LogInformation("[DEMO] Email would be sent to user {UserId}: {Title}",
                notification.UserId, notification.Title);
            await Task.Delay(10);
            return true;
        }
    }
}
