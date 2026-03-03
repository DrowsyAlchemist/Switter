using NotificationService.Models;

namespace NotificationService.Interfaces.Data
{
    public interface INotificationRepository
    {
        Task<Notification?> GetByIdAsync(Guid id);
        Task<List<Notification>> GetByUserAsync(Guid userId, int page, int pageSize);
        Task<List<Notification>> GetUnreadByUserAsync(Guid userId, int page, int pageSize);
        Task<int> GetUnreadCountByUserAsync(Guid userId);

        Task<int> MarkAllAsReadAsync(Guid userId);
        Task<Notification> AddAsync(Notification notification);
        Task AddAsync(List<Notification> notifications);
        Task<Notification> UpdateAsync(Notification notification);
        Task RemoveAsync(Guid id);
    }
}
