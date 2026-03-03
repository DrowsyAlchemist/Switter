using NotificationService.Models;

namespace NotificationService.DTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public NotificationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // Metadata
        public Guid? SourceUserId { get; set; }
        public string? SourceUserName { get; set; }
        public string? SourceUserAvatarUrl { get; set; }
        public Guid? SourceTweetId { get; set; }
    }
}
