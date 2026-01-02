using System.ComponentModel.DataAnnotations;

namespace TweetService.Models
{
    public class Tweet
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public bool IsDeleted { get; set; }

        public required Guid AuthorId { get; set; }
        public required string AuthorDisplayName { get; set; }
        public string? AuthorAvatarUrl { get; set; }

        [Required]
        [MaxLength(280)]
        public required string Content { get; set; }

        public Guid? ParentTweetId { get; set; } // For replies and retweets
        public TweetType Type { get; set; } = TweetType.Tweet;

        public int LikesCount { get; set; }
        public int RetweetsCount { get; set; }
        public int RepliesCount { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Tweet? ParentTweet { get; set; }
        public virtual ICollection<Tweet> Replies { get; set; } = new List<Tweet>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        public virtual ICollection<TweetHashtag> TweetHashtags { get; set; } = new List<TweetHashtag>();
    }
}
