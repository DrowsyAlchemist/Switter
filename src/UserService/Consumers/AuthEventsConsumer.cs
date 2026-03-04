using Microsoft.Extensions.Options;
using NotificationService.Consumers;
using System.Text.Json;
using UserService.Interfaces.Data;
using UserService.KafkaEvents.AuthEvents;
using UserService.Models;
using UserService.Models.Options;

namespace UserService.Consumers
{
    public class AuthEventsConsumer : EventsConsumer
    {
        private readonly IServiceProvider _serviceProvider;

        public AuthEventsConsumer(
            IServiceProvider serviceProvider,
            IOptions<KafkaOptions> kafkaOptions,
            ILogger<AuthEventsConsumer> logger)
            : base(kafkaOptions, logger)
        {
            _serviceProvider = serviceProvider;
        }

        protected override IEnumerable<string> GetTopics()
        {
            return [Options.AuthEvents.UserRegisteredEventName];
        }

        protected override async Task ProcessMessageAsync(string topic, string message, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var profilesRepository = scope.ServiceProvider.GetRequiredService<IProfilesRepository>();

            try
            {
                var userEvent = JsonSerializer.Deserialize<UserRegisteredEvent>(message);
                if (userEvent != null)
                {
                    var profile = new UserProfile
                    {
                        Id = userEvent.UserId,
                        DisplayName = userEvent.Username,
                        CreatedAt = userEvent.Timestamp,
                        UpdatedAt = userEvent.Timestamp
                    };
                    await profilesRepository.AddAsync(profile);
                    Logger.LogInformation("Created profile for user {UserId}", userEvent.UserId);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing user event");
            }
        }
    }
}