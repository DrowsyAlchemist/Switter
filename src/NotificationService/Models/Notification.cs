using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models
{
    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } // Recipient

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; } = NotificationType.System;
        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
        public string ErrorMessage { get; set; } = string.Empty;

        // Metadata
        public Guid? SourceUserId { get; set; }
        public string? SourceUserName { get; set; }
        public string? SourceUserAvatarUrl { get; set; }
        public Guid? SourceTweetId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }
        public DateTime? ReadAt { get; set; }

        // Channels
        public bool ShouldSendPush { get; set; }
        public bool ShouldSendEmail { get; set; }
        public bool ShouldSendWebSocket { get; set; } = true;
    }
}
