namespace FeedService.Events
{
    public record UserBlockedEvent(Guid BlockerId, Guid BlockedId, DateTime Timestamp);
}
