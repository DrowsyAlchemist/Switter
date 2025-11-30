using UserService.DTOs;
using UserService.Interfaces.Commands;
using UserService.Interfaces.Infrastructure;

namespace UserService.Services.Decorators
{
    public class ProfileCommandsCached : IProfileCommands
    {
        private IProfileCommands _profileCommands;
        private IRedisService _redisService;

        public ProfileCommandsCached(IProfileCommands profileCommands, IRedisService redisService)
        {
            _profileCommands = profileCommands;
            _redisService = redisService;
        }

        public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
        {
            var updatedProfile = await _profileCommands.UpdateProfileAsync(userId, request);
            var cacheKey = GetRedisKey(userId);
            await _redisService.RemoveAsync(cacheKey);
            return updatedProfile;
        }

        private static string GetRedisKey(Guid userId) => $"profile:{userId}";
    }
}
