using UserService.DTOs;
using UserService.Interfaces;
using UserService.Interfaces.Infrastructure;
using UserService.KafkaEvents.UserEvents;

namespace UserService.Services.Decorators
{
    public class BlockServiceWithKafka : IBlockService
    {
        private readonly IBlockService _blockService;
        private readonly IKafkaProducer _kafkaProducer;

        public BlockServiceWithKafka(IBlockService blockService, IKafkaProducer kafkaProducer)
        {
            _blockService = blockService;
            _kafkaProducer = kafkaProducer;
        }

        public async Task BlockAsync(Guid blocker, Guid blocked)
        {
            var blockedEvent = new UserBlockedEvent(blocker, blocked, DateTime.UtcNow);
            await _blockService.BlockAsync(blocker, blocked);
            await _kafkaProducer.ProduceAsync("user-blocked", blockedEvent);
        }

        public async Task UnblockAsync(Guid blocker, Guid blocked)
        {
            var blockedEvent = new UserBlockedEvent(blocker, blocked, DateTime.UtcNow);
            await _blockService.UnblockAsync(blocker, blocked);
            await _kafkaProducer.ProduceAsync("user-unblocked", blockedEvent);
        }

        public async Task<List<UserProfileDto>> GetBlockedAsync(Guid blockerId, int page = 1, int pageSize = 20)
        {
            return await _blockService.GetBlockedAsync(blockerId, page, pageSize);
        }

        public async Task<bool> IsBlockedAsync(Guid blocker, Guid blocked)
        {
            return await _blockService.IsBlockedAsync(blocker, blocked);
        }
    }
}
