namespace NotificationService.Models
{
    public class UserNotificationSettings
    {
        public Guid UserId { get; set; }

        public bool EnableLikeNotifications { get; set; } = true;
        public bool EnableRetweetNotifications { get; set; } = true;
        public bool EnableReplyNotifications { get; set; } = true;
        public bool EnableFollowNotifications { get; set; } = true;
        public bool EnableMessageNotifications { get; set; } = true;
        public bool EnableSystemNotifications { get; set; } = true;

        // Channels
        public bool EnablePushNotifications { get; set; } = false;
        public bool EnableEmailNotifications { get; set; } = false;
        public bool EnableWebSocketNotifications { get; set; } = true;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
