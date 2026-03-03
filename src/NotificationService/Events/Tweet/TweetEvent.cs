namespace NotificationService.Events.Tweet
{
    public record class TweetEvent(string EventType, Guid TweetId, Guid InitiatorUserId);
}
