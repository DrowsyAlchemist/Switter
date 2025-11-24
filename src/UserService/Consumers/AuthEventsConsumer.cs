using Confluent.Kafka;
using System.Text.Json;
using UserService.Data;
using UserService.KafkaEvents.AuthEvents;
using UserService.Models;

namespace UserService.Consumers
{
    public class AuthEventsConsumer : BackgroundService
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuthEventsConsumer> _logger;

        public AuthEventsConsumer(IConfiguration configuration, IServiceProvider serviceProvider,
                                ILogger<AuthEventsConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            var config = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                GroupId = "user-service-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe("user-registered-event");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);
                    var message = consumeResult.Message.Value;
                    _logger.LogInformation("Received message: {Message}", message);
                    await ProcessMessageAsync(message);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing Kafka message");
                }
            }
            _consumer.Close();
        }

        private async Task ProcessMessageAsync(string message)
        {
            using var scope = _serviceProvider.CreateScope();
            var profilesRepository = scope.ServiceProvider.GetRequiredService<ProfilesRepository>();

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
                    _logger.LogInformation("Created profile for user {UserId}", userEvent.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing user event");
            }
        }
    }
}