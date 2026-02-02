using System.ComponentModel.DataAnnotations;

namespace TweetService.Models
{
    public class Hashtag
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Length(3, 50)]
        public required string Tag { get; set; }

        public int UsageCount { get; set; } = 1;
        public DateTime FirstUsed { get; set; } = DateTime.UtcNow;
        public DateTime LastUsed { get; set; } = DateTime.UtcNow;

        public virtual ICollection<TweetHashtag> TweetHashtags { get; set; } = new List<TweetHashtag>();
    }
}
