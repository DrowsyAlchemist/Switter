using NotificationService.Models;

namespace NotificationService.Interfaces.Data
{
    public interface INotificationSettingsRepository
    {
        Task<UserNotificationSettings> GetAsync(Guid userId);
        Task<UserNotificationSettings> UpdateAsync(UserNotificationSettings notificationSettings);
    }
}
