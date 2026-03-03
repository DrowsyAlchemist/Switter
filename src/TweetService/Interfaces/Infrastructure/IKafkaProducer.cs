namespace TweetService.Interfaces.Infrastructure
{
    public interface IKafkaProducer
    {
        Task ProduceAsync<T>(string topic, T message) where T : class;
    }
}
