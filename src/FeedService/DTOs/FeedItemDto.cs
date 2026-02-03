using FeedService.Models;

namespace FeedService.DTOs
{
    public class FeedItemDto
    {
        public required Guid TweetId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public required TweetType TweetType { get; set; } = TweetType.Tweet;
        public Guid? ParentTweetId { get; set; }

        public required Guid AuthorId { get; set; }
        public required string AuthorDisplayName { get; set; } = string.Empty;
        public string AuthorAvatarUrl { get; set; } = string.Empty;

        public int LikesCount { get; set; }
        public int RetweetsCount { get; set; }
        public int RepliesCount { get; set; }

        public bool IsLiked { get; set; }
        public bool IsRetweeted { get; set; }
    }
}
