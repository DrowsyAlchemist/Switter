namespace FeedService.Events
{
    public record UserFollowedEvent(Guid FollowerId, Guid FolloweeId, DateTime Timestamp);
}
