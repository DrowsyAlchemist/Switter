using TweetService.Models;

namespace TweetService.Events
{
    public record class TweetCreatedEvent(Guid Id, Guid AuthorId, TweetType Type, DateTime Timestamp);
}
