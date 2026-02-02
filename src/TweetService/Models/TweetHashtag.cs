namespace TweetService.Models
{
    public class TweetHashtag
    {
        public required Guid TweetId { get; set; }
        public required Guid HashtagId { get; set; }

        public virtual Tweet Tweet { get; set; } = null!;
        public virtual Hashtag Hashtag { get; set; } = null!;
    }

}
