namespace UserService.Events
{
    public abstract class KafkaEvent
    {
        public string? Message { get; protected set; }
    }
}
