using UserService.Interfaces.Commands;
using UserService.Interfaces.Infrastructure;
using UserService.KafkaEvents.UserEvents;

namespace UserService.Services.Decorators
{
    public class BlockCommandsWithKafka : IBlockCommands
    {
        private readonly IBlockCommands _blockCommands;
        private readonly IKafkaProducer _kafkaProducer;

        public BlockCommandsWithKafka(IBlockCommands blockCommands, IKafkaProducer kafkaProducer)
        {
            _blockCommands = blockCommands;
            _kafkaProducer = kafkaProducer;
        }

        public async Task BlockAsync(Guid blocker, Guid blocked)
        {
            var blockedEvent = new UserBlockedEvent(blocker, blocked, DateTime.UtcNow);
            await _blockCommands.BlockAsync(blocker, blocked);
            await _kafkaProducer.ProduceAsync("user-blocked", blockedEvent);
        }

        public async Task UnblockAsync(Guid blocker, Guid blocked)
        {
            var blockedEvent = new UserBlockedEvent(blocker, blocked, DateTime.UtcNow);
            await _blockCommands.UnblockAsync(blocker, blocked);
            await _kafkaProducer.ProduceAsync("user-unblocked", blockedEvent);
        }
    }
}
