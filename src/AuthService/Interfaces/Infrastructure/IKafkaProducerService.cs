namespace AuthService.Interfaces.Infrastructure
{
    public interface IKafkaProducerService
    {
        Task ProduceAsync(string topic, string message);
    }
}
