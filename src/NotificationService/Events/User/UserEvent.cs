namespace NotificationService.Events.User
{
    public record class UserEvent(string EventType, Guid UserId);
}
