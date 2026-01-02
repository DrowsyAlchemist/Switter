using Confluent.Kafka;
using System.Text.Json;
using TweetService.Events;
using TweetService.Interfaces.Data;

namespace TweetService.Consumers
{
    public class UserEventsConsumer : BackgroundService
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UserEventsConsumer> _logger;

        public UserEventsConsumer(IConfiguration configuration, IServiceProvider serviceProvider,
                                ILogger<UserEventsConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            var config = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                GroupId = "tweet-service-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoOffsetStore = false,
                EnableAutoCommit = false
            };
            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(["user-profile-changed", "user-profile-deleted"]);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);
                    var message = consumeResult.Message.Value;
                    var topic = consumeResult.Topic;

                    _logger.LogInformation("Received message from topic {Topic}: {Message}", topic, message);

                    await ProcessMessageAsync(topic, message);

                    _consumer.StoreOffset(consumeResult);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming Kafka message");
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Consuming cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error processing Kafka message");
                }
            }
            _consumer.Close();
            _consumer.Dispose();
        }

        private async Task ProcessMessageAsync(string topic, string message)
        {
            using var scope = _serviceProvider.CreateScope();

            try
            {
                switch (topic)
                {
                    case "user-profile-changed":
                        await HandleUserProfileChanged(message, scope.ServiceProvider);
                        break;

                    case "user-profile-deleted":
                        await HandleUserProfileDeleted(message, scope.ServiceProvider);
                        break;

                    default:
                        _logger.LogWarning("Unknown topic: {Topic}", topic);
                        break;
                }
                _logger.LogInformation("Successfully processed message from topic {Topic}", topic);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing message from topic {Topic}: {Message}",
                    topic, message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from topic {Topic}", topic);
                throw;
            }
        }

        private async Task HandleUserProfileChanged(string message, IServiceProvider services)
        {
            var userEvent = JsonSerializer.Deserialize<UserProfileChangedEvent>(message);

            if (userEvent == null)
                throw new InvalidOperationException("Failed to deserialize user profile changed event");

            _logger.LogInformation("Processing user profile update for user {UserId}", userEvent.UserId);

            var tweetRepository = services.GetRequiredService<ITweetRepository>();
            var userTweets = await tweetRepository.GetByUserAsync(userEvent.UserId);

            foreach (var tweet in userTweets)
            {
                if (userEvent.DisplayName != null)
                    tweet.AuthorDisplayName = userEvent.DisplayName;

                if (userEvent.AvatarUrl != null)
                    tweet.AuthorDisplayName = userEvent.AvatarUrl;
            }
            await tweetRepository.UpdateRangeAsync(userTweets);
        }

        private async Task HandleUserProfileDeleted(string message, IServiceProvider services)
        {
            var userEvent = JsonSerializer.Deserialize<UserProfileChangedEvent>(message);

            if (userEvent == null)
                throw new InvalidOperationException("Failed to deserialize user profile changed event");

            _logger.LogInformation("Processing user profile update for user {UserId}", userEvent.UserId);

            var tweetRepository = services.GetRequiredService<ITweetRepository>();
            var userTweetIds = await tweetRepository.GetIdsByUserAsync(userEvent.UserId);
            await tweetRepository.DeleteRangeAsync(userTweetIds);
        }
    }
}