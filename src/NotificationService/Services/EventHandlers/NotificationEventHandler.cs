using Microsoft.Extensions.Options;
using NotificationService.Interfaces;
using NotificationService.Interfaces.Infrastructure;
using NotificationService.Models.Options;
using System.Text.Json;

namespace NotificationService.Services.EventHandlers
{
    public abstract class NotificationEventHandler : INotificationEventHandler
    {
        protected readonly INotificationDeliveryService DeliveryService;
        protected readonly IProfileServiceClient ProfileService;
        protected readonly KafkaOptions Options;

        public NotificationEventHandler(
            INotificationEventsProcessor eventProcessor,
            INotificationDeliveryService deliveryService,
            IProfileServiceClient profileService,
            IOptions<KafkaOptions> options)
        {
            DeliveryService = deliveryService;
            ProfileService = profileService;
            Options = options.Value;

            eventProcessor.Subscribe(HandleAsync);
        }

        public abstract Task HandleAsync(string eventName, string eventJson);

        protected T DeserializeEvent<T>(string message)
        {
            var @event = JsonSerializer.Deserialize<T>(message);

            if (@event == null)
                throw new JsonException($"Failed to deserialize {nameof(T)} event");

            return @event;
        }
    }
}
