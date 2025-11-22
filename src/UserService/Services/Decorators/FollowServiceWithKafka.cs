using UserService.DTOs;
using UserService.Interfaces;
using UserService.Interfaces.Infrastructure;
using UserService.KafkaEvents.UserEvents;

namespace UserService.Services.Decorators
{
    public class FollowServiceWithKafka : IFollowService
    {
        private readonly IFollowService _followService;
        private readonly IKafkaProducer _kafkaProducer;

        public FollowServiceWithKafka(IFollowService followService, IKafkaProducer kafkaProducer)
        {
            _followService = followService;
            _kafkaProducer = kafkaProducer;
        }

        public async Task FollowUserAsync(Guid followerId, Guid followeeId)
        {
            var blockedEvent = new UserFollowedEvent(followerId, followeeId, DateTime.UtcNow);
            await _followService.FollowUserAsync(followerId, followeeId);
            await _kafkaProducer.ProduceAsync("user-followed", blockedEvent);
        }

        public async Task UnfollowUserAsync(Guid followerId, Guid followeeId)
        {
            var blockedEvent = new UserUnfollowedEvent(followerId, followeeId, DateTime.UtcNow);
            await _followService.UnfollowUserAsync(followerId, followeeId);
            await _kafkaProducer.ProduceAsync("user-unfollowed", blockedEvent);
        }

        public async Task<List<UserProfileDto>> GetFollowersAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            return await _followService.GetFollowersAsync(userId, page, pageSize);
        }

        public async Task<List<UserProfileDto>> GetFollowingAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            return await _followService.GetFollowingAsync(userId, page, pageSize);
        }

        public async Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId)
        {
            return await _followService.IsFollowingAsync(followerId, followeeId);
        }
    }
}
