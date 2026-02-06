namespace FeedService.Models
{
    public class UserFeed
    {
        public Guid UserId { get; set; }
        public List<FeedItem> Items { get; set; } = new();
        public DateTime LastUpdated { get; set; }
        public int TotalCount { get; set; }
    }
}
