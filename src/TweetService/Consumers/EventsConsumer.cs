using Confluent.Kafka;
using Microsoft.Extensions.Options;
using TweetService.Models.Options;

namespace TweetService.Consumers
{
    public abstract class EventsConsumer : BackgroundService
    {
        protected readonly KafkaOptions Options;
        protected readonly ILogger<EventsConsumer> Logger;
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly TimeSpan _pollTimeout = TimeSpan.FromMicroseconds(100);

        public EventsConsumer(IOptions<KafkaOptions> options, ILogger<EventsConsumer> logger)
        {
            Options = options.Value;
            Logger = logger;

            var config = new ConsumerConfig
            {
                BootstrapServers = Options.BootstrapServers,
                GroupId = Options.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoOffsetStore = false,
                EnableAutoCommit = false,
                AllowAutoCreateTopics = true,
                MaxPollIntervalMs = 300000,
                SessionTimeoutMs = 10000,
                HeartbeatIntervalMs = 3000
            };
            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        }

        protected abstract IEnumerable<string> GetTopics();

        protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var topics = GetTopics();
            if (topics == null || topics.Any() == false)
                throw new ArgumentException(nameof(topics));

            _consumer.Subscribe(topics);
            Logger.LogInformation("Kafka consumer started for topics: {topics}", string.Join(", ", topics));

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

                        Logger.LogInformation("Received message from topic {Topic}: {Message}", topic, message);

                        await ProcessMessageAsync(topic, message, stoppingToken);

                        _consumer.StoreOffset(consumeResult);
                        Logger.LogDebug("Offset committed for topic {Topic}, partition {Partition}, offset {Offset}",
                                topic, consumeResult.Partition.Value, consumeResult.Offset.Value);
                    }
                    catch (ConsumeException ex)
                    {
                        Logger.LogError(ex, "Error consuming Kafka message");
                        await Task.Delay(5000, stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        Logger.LogInformation("Consuming cancelled");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Unexpected error processing Kafka message");
                        await Task.Delay(10000, stoppingToken);
                    }
                }
                Logger.LogInformation("Closing Kafka consumer...");
                _consumer.Close();
                _consumer.Dispose();
                Logger.LogInformation("Kafka consumer disposed");
            }, stoppingToken);
            await task;
        }

        protected abstract Task ProcessMessageAsync(string topic, string message, CancellationToken cancellationToken);
    }
}