using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class UserProfile
    {
        public required Guid Id { get; set; } // Same with AuthService

        [Length(3, 50)]
        public required string DisplayName { get; set; }

        [MaxLength(500)]
        public string Bio { get; set; } = string.Empty;

        public string AvatarUrl { get; set; } = string.Empty;
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Follow> Followers { get; set; } = new List<Follow>();
        public virtual ICollection<Follow> Following { get; set; } = new List<Follow>();
    }
}
