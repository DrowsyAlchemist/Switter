namespace FeedService.DTOs
{
    public class FeedItemDto
    {
        public Guid TweetId { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int LikesCount { get; set; }
        public int RetweetsCount { get; set; }
        public bool IsLiked { get; set; }
        public bool IsRetweeted { get; set; }
    }
}
