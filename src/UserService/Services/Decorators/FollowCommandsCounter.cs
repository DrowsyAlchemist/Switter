using UserService.Exceptions.Profiles;
using UserService.Interfaces.Commands;
using UserService.Interfaces.Data;
using UserService.Interfaces.Infrastructure;

namespace UserService.Services.Decorators
{
    public class FollowCommandsCounter : IFollowCommands
    {
        private readonly IProfilesRepository _profilesRepository;
        private readonly IRedisService _redis;
        private readonly IFollowCommands _followCommands;

        public FollowCommandsCounter(IProfilesRepository profilesRepository, IRedisService redis, IFollowCommands followCommands)
        {
            _profilesRepository = profilesRepository;
            _redis = redis;
            _followCommands = followCommands;
        }

        public async Task FollowUserAsync(Guid followerId, Guid followeeId)
        {
            await _followCommands.FollowUserAsync(followerId, followeeId);
            await UpdateCounter(followerId, followeeId, value: 1);
        }

        public async Task UnfollowUserAsync(Guid followerId, Guid followeeId)
        {
            await _followCommands.UnfollowUserAsync(followerId, followeeId);
            await UpdateCounter(followerId, followeeId, value: -1);
        }

        private static string GetRedisKey(Guid userId) => $"profile:{userId}";

        private async Task UpdateCounter(Guid followerId, Guid followeeId, int value)
        {
            await _redis.RemoveAsync(GetRedisKey(followerId));
            await _redis.RemoveAsync(GetRedisKey(followeeId));

            var followerProfile = await _profilesRepository.GetProfileByIdAsync(followerId);
            var followeeProfile = await _profilesRepository.GetProfileByIdAsync(followeeId);

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
