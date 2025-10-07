using AuthService.Interfaces.Infrastructure;
using Confluent.Kafka;

namespace AuthService.Services.Infrastructure
{
    public class KafkaProducerService : IKafkaProducerService
    {
        private readonly IProducer<Null, string> _producer;
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"]
            };

            _producer = new ProducerBuilder<Null, string>(config).Build();
            _logger = logger;
        }

        public async Task ProduceAsync(string topic, string message)
        {
            if (string.IsNullOrEmpty(topic)) throw new ArgumentException("Topic required");
            if (string.IsNullOrEmpty(message)) throw new ArgumentException("Message required");

            try
            {
                var result = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
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
