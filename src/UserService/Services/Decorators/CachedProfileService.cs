using System.Text.Json;
using UserService.DTOs;
using UserService.Interfaces;
using UserService.Interfaces.Infrastructure;

namespace UserService.Services.Decorators
{
    public class CachedProfileService : IUserProfileService
    {
        private IUserProfileService _profileService;
        private IRedisService _redisService;
        private ILogger<CachedProfileService> _logger;

        public CachedProfileService(IUserProfileService profileService, IRedisService redisService, ILogger<CachedProfileService> logger)
        {
            _profileService = profileService;
            _redisService = redisService;
            _logger = logger;
        }

        public async Task<UserProfileDto> GetProfileAsync(Guid userId, Guid? currentUserId = null)
        {
            var cacheKey = GetRedisKey(userId);
            UserProfileDto? profileDto = await GetProfileFromCacheAsync(cacheKey);

            if (profileDto != null)
                return profileDto;

            profileDto = await _profileService.GetProfileAsync(userId);

            await _redisService.SetAsync(cacheKey,
                JsonSerializer.Serialize(profileDto),
                TimeSpan.FromMinutes(5));

            return profileDto;
        }

        public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
        {
            var updatedProfile = await _profileService.UpdateProfileAsync(userId, request);
            var cacheKey = GetRedisKey(userId);
            await _redisService.RemoveAsync(cacheKey);
            return updatedProfile;
        }

        public Task<List<UserProfileDto>> SearchUsersAsync(string query, int page = 1, int pageSize = 20)
        {
            return _profileService.SearchUsersAsync(query, page, pageSize);
        }

        private static string GetRedisKey(Guid userId) => $"profile:{userId}";

        private async Task<UserProfileDto?> GetProfileFromCacheAsync(string cacheKey)
        {

            var cachedProfile = await _redisService.GetAsync(cacheKey);
            UserProfileDto? profileDto = null;

            if (string.IsNullOrEmpty(cachedProfile) == false)
            {
                try
                {
                    profileDto = JsonSerializer.Deserialize<UserProfileDto>(cachedProfile);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Cached data is invalid.\nKey: {cacheKey}\nCachedProfile: {cachedProfile}");
                }
            }
            return profileDto;
        }
    }
}
