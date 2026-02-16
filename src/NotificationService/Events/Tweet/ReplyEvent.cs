namespace NotificationService.Events.Tweet
{
    public record class ReplyEvent(string EventType, Guid TweetId, Guid AuthorId, Guid ParentTweetId, Guid ParentTweetAuthorId) :
        TweetEvent(EventType, TweetId, AuthorId);
}
