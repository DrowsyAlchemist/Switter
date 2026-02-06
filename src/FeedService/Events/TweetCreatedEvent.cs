namespace FeedService.Events
{
    public record class TweetCreatedEvent(Guid Id, Guid AuthorId, DateTime Timestamp);
}
