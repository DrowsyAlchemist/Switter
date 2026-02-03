namespace FeedService.Events
{
    public record class RetweetCreatedEvent(Guid Id, Guid AuthorId, Guid ParentTweet, DateTime Timestamp);
}
