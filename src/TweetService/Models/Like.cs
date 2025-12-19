namespace TweetService.Models
{
    public class Like
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Guid TweetId { get; set; }
        public required Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Tweet Tweet { get; set; } = null!;
    }
}
