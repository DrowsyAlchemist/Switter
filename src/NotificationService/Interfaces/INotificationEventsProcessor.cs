namespace NotificationService.Interfaces
{
    public interface INotificationEventsProcessor
    {
        Task ProcessEventAsync(string eventName, string eventJson);
        void Subscribe(Func<string, string, Task> handler);
    }
}
