namespace UserService.KafkaEvents.UserEvents
{
    public class UserUnblockedEvent(Guid BlockerId, Guid BlockedId, DateTime Timestamp);
}
