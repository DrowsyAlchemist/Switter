using Confluent.Kafka;
using System.Text.Json;
using System.Threading.Channels;
using UserService.Interfaces.Data;
using UserService.KafkaEvents.AuthEvents;
using UserService.Models;

namespace UserService.Consumers
{
    public class AuthEventsConsumer : BackgroundService
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuthEventsConsumer> _logger;
        private readonly Channel<string> _messageChannel;

        public AuthEventsConsumer(IConfiguration configuration, IServiceProvider serviceProvider,
                                ILogger<AuthEventsConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _messageChannel = Channel.CreateUnbounded<string>();

            var config = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                GroupId = "user-service-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                AllowAutoCreateTopics = true
            };
            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumingTask = Task.Run(async () =>
            {
                _consumer.Subscribe("user-registered-event");

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(TimeSpan.FromMilliseconds(1000));
                        if (consumeResult != null)
                        {
                            await _messageChannel.Writer.WriteAsync(consumeResult.Message.Value, stoppingToken);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error consuming from Kafka");
                        await Task.Delay(5000, stoppingToken);
                    }
                }
            }, stoppingToken);

            var processingTask = Task.Run(async () =>
            {
                await foreach (var message in _messageChannel.Reader.ReadAllAsync(stoppingToken))
                {
                    try
                    {
                        await ProcessMessageAsync(message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message");
                    }
                }
            }, stoppingToken);

            await Task.WhenAll(consumingTask, processingTask);
        }

        private async Task ProcessMessageAsync(string message)
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
                    _logger.LogInformation("Created profile for user {UserId}", userEvent.UserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing user event");
            }
        }

        public override void Dispose()
        {
            _consumer?.Close();
            _consumer?.Dispose();
            base.Dispose();
        }
    }
}