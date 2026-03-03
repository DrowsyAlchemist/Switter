namespace FeedService.Events
{
    public record UserUnfollowedEvent(Guid FollowerId, Guid FolloweeId, DateTime Timestamp);
}
