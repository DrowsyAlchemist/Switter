using Microsoft.Extensions.Diagnostics.HealthChecks;
using NotificationService.Interfaces;
using NotificationService.Interfaces.Data;
using NotificationService.Models;

namespace NotificationService.HealthChecks
{
    public class DeliveryServiceHealthCheck : IHealthCheck
    {

        private readonly INotificationDeliveryService _deliveryService;
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<DeliveryServiceHealthCheck> _logger;

        public DeliveryServiceHealthCheck(
            INotificationDeliveryService deliveryService,
            INotificationRepository notificationRepository,
            ILogger<DeliveryServiceHealthCheck> logger)
        {
            _deliveryService = deliveryService;
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var testUserId = Guid.Empty;
                var notification = new Notification
                {
                    UserId = testUserId,
                    Title = "Новое уведомление",
                    Message = Guid.NewGuid().ToString(),
                    Type = NotificationType.System,
                    ShouldSendWebSocket = true,
                    ShouldSendPush = false,
                    ShouldSendEmail = false
                };
                await _deliveryService.DeliverNotificationAsync(notification);

                var unreadNotifications = await _notificationRepository.GetUnreadByUserAsync(testUserId, 1, int.MaxValue);

                var notificationIdDb = unreadNotifications.FirstOrDefault(n => n.Message == notification.Message);

                bool isHealthy = notificationIdDb != null && notification.Status == NotificationStatus.Unread;

                await _notificationRepository.RemoveAsync(notificationIdDb!.Id);

                return isHealthy
                       ? HealthCheckResult.Healthy("Tweet service is working")
                       : HealthCheckResult.Unhealthy("Tweet service has problems"); ;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tweet service health check failed");
                return HealthCheckResult.Unhealthy("Tweet service exception");
            }
        }
    }
}
