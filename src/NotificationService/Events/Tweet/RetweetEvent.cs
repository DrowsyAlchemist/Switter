namespace NotificationService.Events.Tweet
{
    public record class RetweetEvent(string EventType, Guid TweetId, Guid AuthorId, Guid ParentTweetId, Guid ParentTweetAuthorId) :
        TweetEvent(EventType, TweetId, AuthorId);
}
