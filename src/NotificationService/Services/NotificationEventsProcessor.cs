using NotificationService.Interfaces;

namespace NotificationService.Services
{
    public class NotificationEventsProcessor : INotificationEventsProcessor
    {
        private readonly List<Func<string, string, Task>> _handlers = new();
        private readonly ILogger<NotificationEventsProcessor> _logger;

        public NotificationEventsProcessor(ILogger<NotificationEventsProcessor> logger)
        {
            _logger = logger;
        }

        public void Subscribe(Func<string, string, Task> handler)
        {
            _handlers.Add(handler);
        }

        public async Task ProcessEventAsync(string eventName, string eventJson)
        {
            var tasks = _handlers.Select(h => SafeInvoke(h, eventName, eventJson));
            await Task.WhenAll(tasks);
        }

        private async Task SafeInvoke(Func<string, string, Task> handler, string eventName, string eventJson)
        {
            try
            {
                await handler(eventName, eventJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in eventHandler: {ex}", ex);
            }
        }
    }
}
