namespace TweetService.Models
{
    public class RetweetUsage
    {
        public required Guid ParentTweetId { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    }
}
