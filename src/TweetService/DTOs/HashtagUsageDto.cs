namespace TweetService.DTOs
{
    public class HashtagUsage
    {
        public string Tag { get; set; } = string.Empty;
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    }
}
