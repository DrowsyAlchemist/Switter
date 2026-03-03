using Microsoft.EntityFrameworkCore;
using NotificationService.Interfaces.Data;
using NotificationService.Models;

namespace NotificationService.Data
{
    public class NotificationSettingsRepository : INotificationSettingsRepository
    {
        private readonly NotificationDbContext _context;

        public NotificationSettingsRepository(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<UserNotificationSettings> GetAsync(Guid userId)
        {
            var settings = await _context.UserNotificationSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (settings != null)
                return settings;

            var defaultSettings = new UserNotificationSettings();
            defaultSettings.UserId = userId;
            defaultSettings.UpdatedAt = DateTime.UtcNow;
            return await AddAsync(defaultSettings);
        }

        public async Task<UserNotificationSettings> UpdateAsync(UserNotificationSettings notificationSettings)
        {
            ArgumentNullException.ThrowIfNull(notificationSettings);
            var localSettings = await _context.UserNotificationSettings.FindAsync(notificationSettings.UserId);
            if (localSettings == null)
                return await AddAsync(notificationSettings);

            _context.UserNotificationSettings.Entry(localSettings).CurrentValues.SetValues(notificationSettings);
            localSettings.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _context.UserNotificationSettings.Entry(localSettings).State = EntityState.Detached;
            return localSettings;
        }

        private async Task<UserNotificationSettings> AddAsync(UserNotificationSettings notificationSettings)
        {
            ArgumentNullException.ThrowIfNull(notificationSettings);
            await _context.UserNotificationSettings.AddAsync(notificationSettings);
            await _context.SaveChangesAsync();
            _context.UserNotificationSettings.Entry(notificationSettings).State = EntityState.Detached;
            return notificationSettings;
        }
    }
}
