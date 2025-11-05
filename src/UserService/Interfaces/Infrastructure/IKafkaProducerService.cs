using UserService.Events;

namespace UserService.Interfaces.Infrastructure
{
    public interface IKafkaProducerService
    {
        Task ProduceAsync(string topic, KafkaEvent kafkaEvent);
    }
}
