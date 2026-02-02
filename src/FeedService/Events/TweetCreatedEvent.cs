using FeedService.Models;

namespace FeedService.Events
{
    public record class TweetCreatedEvent(Guid Id, Guid AuthorId, TweetType Type, DateTime Timestamp);
}
