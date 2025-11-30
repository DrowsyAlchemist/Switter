using UserService.Interfaces.Commands;
using UserService.Interfaces.Infrastructure;
using UserService.KafkaEvents.UserEvents;

namespace UserService.Services.Decorators
{
    public class FollowCommandsWithKafka : IFollowCommands
    {
        private readonly IFollowCommands _followCommands;
        private readonly IKafkaProducer _kafkaProducer;

        public FollowCommandsWithKafka(IFollowCommands followCommands, IKafkaProducer kafkaProducer)
        {
            _followCommands = followCommands;
            _kafkaProducer = kafkaProducer;
        }

        public async Task FollowUserAsync(Guid followerId, Guid followeeId)
        {
            var blockedEvent = new UserFollowedEvent(followerId, followeeId, DateTime.UtcNow);
            await _followCommands.FollowUserAsync(followerId, followeeId);
            await _kafkaProducer.ProduceAsync("user-followed", blockedEvent);
        }

        public async Task UnfollowUserAsync(Guid followerId, Guid followeeId)
        {
            var blockedEvent = new UserUnfollowedEvent(followerId, followeeId, DateTime.UtcNow);
            await _followCommands.UnfollowUserAsync(followerId, followeeId);
            await _kafkaProducer.ProduceAsync("user-unfollowed", blockedEvent);
        }
    }
}