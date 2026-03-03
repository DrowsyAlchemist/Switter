namespace FeedService.Models.Options
{
    public class KafkaOptions
    {
        public required string BootstrapServers { get; set; }
        public required string GroupId { get; set; }
        public required TweetEvents TweetEvents { get; set; }
        public required ProfileEvents ProfileEvents { get; set; }
    }

    public class TweetEvents
    {
        public required string TweetCreatedEventName { get; set; }
        public required string RetweetCreatedEventName { get; set; }
        public required string ReplyCreatedEventName { get; set; }
        public required string TweetDeletedEventName { get; set; }
        public required string LikeSetEventName { get; set; }
        public required string LikeCanceledEventName { get; set; }
    }

    public class ProfileEvents
    {
        public required string ProfileChangedEventName { get; set; }
        public required string ProfileDeletedEventName { get; set; }
        public required string UserFollowedEventName { get; set; }
        public required string UserUnfollowedEventName { get; set; }
        public required string UserBlockedEventName { get; set; }
    }
}
