namespace FeedService.Models
{
    public class FeedItem
    {
        public Guid TweetId { get; set; }
        public Guid AuthorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public double Score { get; set; } // For sort

        public string? CachedContent { get; set; }
        public string? CachedAuthorName { get; set; }
    }
}
