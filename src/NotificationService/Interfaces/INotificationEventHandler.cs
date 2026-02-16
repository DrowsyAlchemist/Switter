namespace NotificationService.Interfaces
{
    public interface INotificationEventHandler
    {
        Task HandleAsync(string eventName, string eventJson);
    }
}
