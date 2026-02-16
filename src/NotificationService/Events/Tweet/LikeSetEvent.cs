namespace NotificationService.Events.Tweet
{
    public record class LikeSetEvent(string EventType, Guid TweetId, Guid UserId, Guid TweetAuthorId) :
        TweetEvent(EventType, TweetId, UserId);
}
