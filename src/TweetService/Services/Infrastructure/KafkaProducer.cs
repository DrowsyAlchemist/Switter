using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TweetService.Interfaces.Infrastructure;
using TweetService.Models.Options;

namespace TweetService.Services.Infrastructure
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<Null, string> _producer;
        private readonly ILogger<KafkaProducer> _logger;

        public KafkaProducer(IOptions<KafkaOptions> options, ILogger<KafkaProducer> logger)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = options.Value.BootstrapServers,
            };
            _producer = new ProducerBuilder<Null, string>(config).Build();
            _logger = logger;
        }

        public async Task ProduceAsync<T>(string topic, T message) where T : class
        {
            if (string.IsNullOrEmpty(topic)) throw new ArgumentException("Topic required");
            if (message == null) throw new ArgumentException("Message required");

            try
            {
                var jsonMessage = JsonSerializer.Serialize(message);
                var kafkaEvent = new Message<Null, string> { Value = jsonMessage };
                var result = await _producer.ProduceAsync(topic, kafkaEvent);
                _logger.LogInformation("Message sent to {Topic} [Partition: {Partition}]", result.Topic, result.Partition);
            }
            catch (ProduceException<Null, string> ex)
            {
                _logger.LogError(ex, "Kafka failed to deliver message to {Topic}", topic);
            }
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }
    }
}
