using Microsoft.EntityFrameworkCore;
using NotificationService.Interfaces.Data;
using NotificationService.Models;

namespace NotificationService.Data
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _context;

        public NotificationRepository(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await _context.Notifications
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<Notification>> GetByUserAsync(Guid userId, int page, int pageSize)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetUnreadByUserAsync(Guid userId, int page, int pageSize)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n =>
                    n.UserId == userId
                    && n.Status == NotificationStatus.Unread)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountByUserAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .CountAsync();
        }

        public async Task<Notification> AddAsync(Notification notification)
        {
            ArgumentNullException.ThrowIfNull(notification);
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            _context.Notifications.Entry(notification).State = EntityState.Detached;
            return notification;
        }

        public async Task AddAsync(List<Notification> notifications)
        {
            ArgumentNullException.ThrowIfNull(notifications);
            if (notifications.Count == 0)
                return;

            await _context.Notifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();
        }

        public async Task<int> MarkAllAsReadAsync(Guid userId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n =>
                    n.UserId == userId
                    && n.Status == NotificationStatus.Unread)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.Status = NotificationStatus.Read;
                notification.ReadAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
            return unreadNotifications.Count;
        }

        public async Task<Notification> UpdateAsync(Notification notification)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var localNotification = await _context.Notifications.FindAsync(notification.Id);
            if (localNotification == null)
                throw new KeyNotFoundException($"Notification {notification.Id} not found.");

            _context.Entry(localNotification).CurrentValues.SetValues(notification);
            await _context.SaveChangesAsync();
            _context.Notifications.Entry(localNotification).State = EntityState.Detached;
            return localNotification;
        }

        public async Task RemoveAsync(Guid id)
        {
            var localNotification = await _context.Notifications.FindAsync(id);
            if (localNotification == null)
                throw new KeyNotFoundException($"Notification {id} not found.");

            _context.Notifications.Remove(localNotification);
            await _context.SaveChangesAsync();
        }
    }
}
