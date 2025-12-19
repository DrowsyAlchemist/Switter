namespace TweetService.Models.Trends
{
    public class HashtagUsage
    {
        public required string Tag { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    }
}
