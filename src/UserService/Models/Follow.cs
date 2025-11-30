namespace UserService.Models
{
    public class Follow
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid FollowerId { get; set; }
        public Guid FolloweeId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигационные свойства
        public virtual UserProfile Follower { get; set; } = null!;
        public virtual UserProfile Followee { get; set; } = null!;
    }
}