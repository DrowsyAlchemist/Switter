namespace FeedService.Models
{
    public class FeedItem
    {
        public required Guid TweetId { get; set; }
        public required double Score { get; set; }
    }
}
