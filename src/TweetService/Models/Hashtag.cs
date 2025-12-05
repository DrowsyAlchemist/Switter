using System.ComponentModel.DataAnnotations;

namespace TweetService.Models
{
    public class Hashtag
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string Tag { get; set; } = string.Empty;

        public int UsageCount { get; set; }
        public DateTime FirstUsed { get; set; } = DateTime.UtcNow;
        public DateTime LastUsed { get; set; } = DateTime.UtcNow;

        public virtual ICollection<TweetHashtag> TweetHashtags { get; set; } = new List<TweetHashtag>();
    }
}
