using NotificationService.DTOs;

namespace NotificationService.Interfaces
{
    public interface INotificationSettingsService
    {
        Task<UserNotificationSettingsDto> GetSettingsAsync(Guid userId);
        Task<UserNotificationSettingsDto> UpdateSettingsAsync(UserNotificationSettingsDto settingsDto);
    }
}
