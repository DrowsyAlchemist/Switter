using System.ComponentModel.DataAnnotations;

namespace TweetService.Models
{
    public class Tweet
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AuthorId { get; set; }

        [Required]
        [MaxLength(280)]
        public string Content { get; set; } = string.Empty;

        public Guid? ParentTweetId { get; set; } // For replies and retweets
        public TweetType Type { get; set; } = TweetType.Tweet;

        public int LikesCount { get; set; }
        public int RetweetsCount { get; set; }
        public int RepliesCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // Навигационные свойства
        public virtual Tweet? ParentTweet { get; set; }
        public virtual ICollection<Tweet> Replies { get; set; } = new List<Tweet>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        public virtual ICollection<Hashtag> Hashtags { get; set; } = new List<Hashtag>();
    }
}
