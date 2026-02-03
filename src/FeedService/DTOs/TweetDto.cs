using FeedService.Models;

namespace FeedService.DTOs
{
    public class TweetDto
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
        public required Guid AuthorId { get; set; }
        public required string AuthorDisplayName { get; set; }
        public string? AuthorAvatarUrl { get; set; }
        public required string Content { get; set; }
        public required TweetType Type { get; set; }
        public Guid? ParentTweetId { get; set; }

        public int LikesCount { get; set; }
        public int RetweetsCount { get; set; }
        public int RepliesCount { get; set; }

        public DateTime CreatedAt { get; set; }
        public bool IsLiked { get; set; } // For current user
        public bool IsRetweeted { get; set; } // For current user
    }

}
