using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class UserProfile
    {
        public Guid Id { get; set; } // Same with AuthService

        [MaxLength(50)]
        public string DisplayName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Bio { get; set; } = string.Empty;

        public string AvatarUrl { get; set; } = string.Empty;
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
