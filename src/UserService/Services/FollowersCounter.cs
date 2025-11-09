using AutoMapper;
using UserService.DTOs;
using UserService.Exceptions;
using UserService.Exceptions.Profiles;
using UserService.Interfaces;
using UserService.Interfaces.Data;
using UserService.Interfaces.Infrastructure;

namespace UserService.Services
{
    public class FollowersCounter : IFollowersCounter
    {
        private readonly IProfilesRepository _profilesRepository;
        private readonly IRedisService _redis;
        private readonly IMapper _mapper;

        public FollowersCounter(IProfilesRepository profilesRepository, IRedisService redis, IMapper mapper)
        {
            _profilesRepository = profilesRepository;
            _redis = redis;
            _mapper = mapper;
        }

        public async Task IncrementCounter(Guid followerId, Guid followeeId)
        {
            await UpdateCounter(followerId, followeeId, value: 1);
        }

        public async Task DecrementCounter(Guid followerId, Guid followeeId)
        {
            await UpdateCounter(followerId, followeeId, value: -1);
        }

        public async Task<UserProfileDto> ForceUpdateCountersForUserAsync(Guid userId)
        {
            var user = await _profilesRepository.GetProfileAsync(userId);

            user.FollowersCount = user.Followers.Count;
            user.FollowingCount = user.Following.Count;

            await _profilesRepository.UpdateProfileAsync(user);

            return _mapper.Map<UserProfileDto>(user);
        }

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

            followerProfile.FollowingCount += value;
            followeeProfile.FollowersCount += value;

            await _profilesRepository.UpdateProfileAsync(followerProfile);
            await _profilesRepository.UpdateProfileAsync(followeeProfile);
        }

        private static string GetRedisKey(Guid userId) => $"profile:{userId}";
    }
}
