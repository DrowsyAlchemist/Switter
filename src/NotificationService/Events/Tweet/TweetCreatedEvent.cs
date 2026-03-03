namespace NotificationService.Events.Tweet
{
    public record class TweetCreatedEvent(string EventType, Guid TweetId, Guid AuthorId):
        TweetEvent(EventType, TweetId, AuthorId);
}
