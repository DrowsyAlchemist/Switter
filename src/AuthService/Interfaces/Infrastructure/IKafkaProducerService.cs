namespace AuthService.Interfaces.Infrastructure
{
    public interface IKafkaProducerService
    {
        Task ProduceAsync<T>(string topic, T message) where T: class;
    }
}
