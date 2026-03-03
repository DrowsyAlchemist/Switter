using System.ComponentModel.DataAnnotations;
using TweetService.Models;

namespace TweetService.DTOs
{
    public class CreateTweetRequest
    {
        [Required]
        [MaxLength(280)]
        public required string Content { get; set; } = string.Empty;
        public required TweetType Type { get; set; } = TweetType.Tweet;
        public Guid? ParentTweetId { get; set; }
    }
}
