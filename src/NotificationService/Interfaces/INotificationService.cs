using NotificationService.DTOs;

namespace NotificationService.Interfaces
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetNotificationsAsync(Guid userId, int page, int pageSize);
        Task<List<NotificationDto>> GetUnreadNotificationsAsync(Guid userId, int page, int pageSize);
        Task<int> GetUnreadCountAsync(Guid userId);

        Task MarkAsReadAsync(Guid notificationId, Guid userId);
        Task<int> MarkAllAsReadAsync(Guid userId);
        Task DeleteNotificationAsync(Guid notificationId, Guid userId);
    }
}
