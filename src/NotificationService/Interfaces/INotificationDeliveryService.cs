using NotificationService.Models;

namespace NotificationService.Interfaces
{
    public interface INotificationDeliveryService
    {
        Task DeliverNotificationAsync(Notification notification);
        Task BroadcastSystemNotificationAsync(string message);
    }
}
