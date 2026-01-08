using TweetService.Models;

namespace TweetService.Events
{
    public record class TweetDeletedEvent(Guid Id, DateTime Timestamp);
}
