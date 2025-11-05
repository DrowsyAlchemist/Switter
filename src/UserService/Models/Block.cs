namespace UserService.Models
{
    public class Block
    {
        public Guid Id { get; set; } = Guid.NewGuid();  
        public Guid BlockerId { get; set; }
        public Guid BlockedId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual UserProfile Blocker { get; set; } = null!;
        public virtual UserProfile Blocked { get; set; } = null!;
    }
}
