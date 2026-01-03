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
        private readonly TimeSpan _pollTimeout = TimeSpan.FromMicroseconds(100);

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
                EnableAutoCommit = false,
                MaxPollIntervalMs = 300000,
                SessionTimeoutMs = 10000,
                HeartbeatIntervalMs = 3000
            };
            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(["user-profile-changed", "user-profile-deleted"]);
            _logger.LogInformation("Kafka consumer started for topics: user-profile-changed, user-profile-deleted");

            var task = Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(_pollTimeout);
                        if (consumeResult == null)
                            continue;

                        var message = consumeResult.Message.Value;
                        var topic = consumeResult.Topic;

                        _logger.LogInformation("Received message from topic {Topic}: {Message}", topic, message);

                        await ProcessMessageAsync(topic, message, stoppingToken);

                        _consumer.StoreOffset(consumeResult);
                        _logger.LogDebug("Offset committed for topic {Topic}, partition {Partition}, offset {Offset}",
                                topic, consumeResult.Partition.Value, consumeResult.Offset.Value);
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Error consuming Kafka message");
                        await Task.Delay(5000, stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Consuming cancelled");
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error processing Kafka message");
                        await Task.Delay(10000, stoppingToken);
                    }
                }
                _logger.LogInformation("Closing Kafka consumer...");
                _consumer.Close();
                _consumer.Dispose();
                _logger.LogInformation("Kafka consumer disposed");
            }, stoppingToken);
            await task;
        }

        private async Task ProcessMessageAsync(string topic, string message, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            try
            {
                switch (topic)
                {
                    case "user-profile-changed":
                        await HandleUserProfileChanged(message, scope.ServiceProvider, cancellationToken);
                        break;

                    case "user-profile-deleted":
                        await HandleUserProfileDeleted(message, scope.ServiceProvider, cancellationToken);
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
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error processing message from topic {Topic}", topic);
            }
        }

        private async Task HandleUserProfileChanged(string message, IServiceProvider services, CancellationToken cancellationToken)
        {
            var userEvent = JsonSerializer.Deserialize<UserProfileChangedEvent>(message);

            if (userEvent == null)
                throw new InvalidOperationException("Failed to deserialize user profile changed event");

            _logger.LogInformation("Processing user profile update for user {UserId}", userEvent.UserId);

            if (userEvent.DisplayName == null && userEvent.AvatarUrl == null)
            {
                _logger.LogInformation("No changes required for user {UserId}", userEvent.UserId);
                return;
            }

            var tweetRepository = services.GetRequiredService<ITweetRepository>();
            var userTweets = await tweetRepository.GetByUserAsync(userEvent.UserId);

            foreach (var tweet in userTweets)
            {
                if (userEvent.DisplayName != null)
                    tweet.AuthorDisplayName = userEvent.DisplayName;

                if (userEvent.AvatarUrl != null)
                    tweet.AuthorAvatarUrl = userEvent.AvatarUrl;
            }
            await tweetRepository.UpdateRangeAsync(userTweets);
        }

        private async Task HandleUserProfileDeleted(string message, IServiceProvider services, CancellationToken cancellationToken)
        {
            var userEvent = JsonSerializer.Deserialize<UserProfileDeletedEvent>(message);

            if (userEvent == null)
                throw new InvalidOperationException("Failed to deserialize user profile changed event");

            _logger.LogInformation("Processing user profile update for user {UserId}", userEvent.UserId);

            var tweetRepository = services.GetRequiredService<ITweetRepository>();
            var userTweetIds = await tweetRepository.GetIdsByUserAsync(userEvent.UserId);

            if (userTweetIds.Any())
            {
                await tweetRepository.DeleteRangeAsync(userTweetIds);
                _logger.LogInformation("Deleted {Count} tweets for user {UserId}", userTweetIds.Count(), userEvent.UserId);
            }
            else
            {
                _logger.LogInformation("No tweets found for user {UserId}", userEvent.UserId);
            }
        }
    }
}