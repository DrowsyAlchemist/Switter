namespace TweetService.Models
{
    public class TweetHashtag
    {
        public Guid TweetId { get; set; }
        public Guid HashtagId { get; set; }

        public virtual Tweet Tweet { get; set; } = null!;
        public virtual Hashtag Hashtag { get; set; } = null!;
    }

}
