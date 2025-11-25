namespace TweetService.Models
{
    public class Like
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TweetId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Tweet Tweet { get; set; } = null!;
    }
}
