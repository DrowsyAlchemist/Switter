namespace TweetService.Models
{
    public class LikeUsage
    {
        public required Guid TweetId { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    }
}
