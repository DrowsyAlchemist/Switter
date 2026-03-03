namespace NotificationService.Exceptions
{
    public class NotificationOwnerNotMatchException(Guid notificationId, Guid userId)
        : Exception($"User {userId} is not the owner of notification {notificationId}.")
    { }
}
