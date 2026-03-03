namespace NotificationService.Exceptions
{
    public class NotificationNotFoundException(Guid id) : Exception($"Notification with id {id} not found.")
    { }
}
