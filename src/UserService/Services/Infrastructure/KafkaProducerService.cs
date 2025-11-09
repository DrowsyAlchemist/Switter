using Confluent.Kafka;
using UserService.Events;
using UserService.Interfaces.Infrastructure;

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

        public async Task ProduceAsync(string topic, KafkaEvent kafkaEvent)
        {
            if (string.IsNullOrEmpty(topic)) throw new ArgumentException("Topic required");
            if (string.IsNullOrEmpty(kafkaEvent.Message)) throw new ArgumentException("Message required");

            try
            {
                var result = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = kafkaEvent.Message });
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
