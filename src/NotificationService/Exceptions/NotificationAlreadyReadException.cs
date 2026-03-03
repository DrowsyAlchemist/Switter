namespace NotificationService.Exceptions
{
    public class NotificationAlreadyReadException(Guid notificationId)
        : Exception($"Notification {notificationId} has already read.")
    { }
}
