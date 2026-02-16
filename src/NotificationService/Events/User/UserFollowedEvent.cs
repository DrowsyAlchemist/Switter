namespace NotificationService.Events.User
{
    public record class UserFollowedEvent(string EventType, Guid FollowedUserId, Guid FollowingUserId)
        : UserEvent(EventType, FollowedUserId);
}
