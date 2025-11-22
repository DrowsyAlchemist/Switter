using AutoMapper;
using UserService.DTOs;
using UserService.Exceptions.Profiles;
using UserService.Interfaces;
using UserService.Interfaces.Data;
using UserService.Interfaces.Infrastructure;

namespace UserService.Services.Decorators
{
    public class FollowWithCounterService : IFollowService
    {
        private readonly IProfilesRepository _profilesRepository;
        private readonly IRedisService _redis;
        private readonly IFollowService _followService;

        public FollowWithCounterService(IProfilesRepository profilesRepository, IRedisService redis, IFollowService followService)
        {
            _profilesRepository = profilesRepository;
            _redis = redis;
            _followService = followService;
        }

        public async Task FollowUserAsync(Guid followerId, Guid followeeId)
        {
            await _followService.FollowUserAsync(followerId, followeeId);
            await UpdateCounter(followerId, followeeId, value: 1);
        }

        public async Task UnfollowUserAsync(Guid followerId, Guid followeeId)
        {
            await _followService.UnfollowUserAsync(followerId, followeeId);
            await UpdateCounter(followerId, followeeId, value: -1);
        }

        public async Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId)
        {
            return await _followService.IsFollowingAsync(followerId, followeeId);
        }

        public async Task<List<UserProfileDto>> GetFollowersAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            return await _followService.GetFollowersAsync(userId, page, pageSize);
        }

        public async Task<List<UserProfileDto>> GetFollowingAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            return await _followService.GetFollowingAsync(userId, page, pageSize);
        }

        private static string GetRedisKey(Guid userId) => $"profile:{userId}";

        private async Task UpdateCounter(Guid followerId, Guid followeeId, int value)
        {
            await _redis.RemoveAsync(GetRedisKey(followerId));
            await _redis.RemoveAsync(GetRedisKey(followeeId));

            var followerProfile = await _profilesRepository.GetProfileAsync(followerId);
            var followeeProfile = await _profilesRepository.GetProfileAsync(followeeId);

            if (followerProfile == null)
                throw new UserNotFoundException(followerId);
            if (followeeProfile == null)
                throw new UserNotFoundException(followeeId);
            if (followerProfile.FollowingCount + value < 0)
                throw new InvalidOperationException();
            if (followeeProfile.FollowersCount + value < 0)
                throw new InvalidOperationException();

            followerProfile.FollowingCount += value;
            followeeProfile.FollowersCount += value;

            await _profilesRepository.UpdateProfileAsync(followerProfile);
            await _profilesRepository.UpdateProfileAsync(followeeProfile);
        }
    }
}
