namespace FeedService.Events
{
    public record class ReplyCreatedEvent(Guid Id, Guid AuthorId, Guid ParentTweet, DateTime Timestamp);
}
