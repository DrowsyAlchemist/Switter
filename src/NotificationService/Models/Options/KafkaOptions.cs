namespace NotificationService.Models.Options
{
    public class KafkaOptions
    {
        public required string BootstrapServers { get; set; }
        public required string GroupId { get; set; }
        public required TweetEvents TweetEvents { get; set; }
        public required UserEvents UserEvents { get; set; }
    }

    public class TweetEvents
    {
        public required string TweetCreatedEventName { get; set; }
        public required string RetweetEventName { get; set; }
        public required string ReplyEventName { get; set; }
        public required string TweetDeletedEventName { get; set; }
        public required string LikeSetEventName { get; set; }
        public required string LikeCanceledEventName { get; set; }
    }

    public class UserEvents
    {
        public required string ProfileChangedEventName { get; set; }
        public required string ProfileDeletedEventName { get; set; }
        public required string UserFollowedEventName { get; set; }
        public required string UserUnfollowedEventName { get; set; }
        public required string UserBlockedEventName { get; set; }
    }
}
