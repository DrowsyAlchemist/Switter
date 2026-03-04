using AuthService.Interfaces.Infrastructure;
using AuthService.Models.Options;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AuthService.Services.Infrastructure
{
    public class KafkaProducerService : IKafkaProducerService
    {
        private readonly IProducer<Null, string> _producer;
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(IOptions<KafkaOptions> options, ILogger<KafkaProducerService> logger)
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
