using TweetService.Models;

namespace TweetService.Events
{
    public record class TweetDeletedEvent(Guid Id, Guid AuthorId, TweetType Type, DateTime Timestamp);
}
