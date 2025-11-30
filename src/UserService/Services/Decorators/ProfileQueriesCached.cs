using Microsoft.Extensions.Options;
using System.Text.Json;
using UserService.DTOs;
using UserService.Interfaces.Infrastructure;
using UserService.Interfaces.Queries;
using UserService.Models;

namespace UserService.Services.Decorators
{
    public class ProfileQueriesCached : IProfileQueries
    {
        private readonly IProfileQueries _profileQueries;
        private readonly IRedisService _redisService;
        private readonly IOptions<UserServiceOptions> _options;
        private readonly ILogger<ProfileQueriesCached> _logger;

        public ProfileQueriesCached(IProfileQueries profileQueries,
            IRedisService redisService,
            IOptions<UserServiceOptions> options,
            ILogger<ProfileQueriesCached> logger)
        {
            _profileQueries = profileQueries;
            _redisService = redisService;
            _options = options;
            _logger = logger;
        }

        public async Task<UserProfileDto> GetProfileAsync(Guid userId, Guid? currentUserId = null)
        {
            var cacheKey = GetRedisKey(userId);
            UserProfileDto? profileDto = await GetProfileFromCacheAsync(cacheKey);

            if (profileDto != null)
                return profileDto;

            profileDto = await _profileQueries.GetProfileAsync(userId);

            await _redisService.SetAsync(cacheKey,
                JsonSerializer.Serialize(profileDto),
                TimeSpan.FromMinutes(_options.Value.ProfileExpiryInMinutes));

            return profileDto;
        }

        public Task<List<UserProfileDto>> SearchUsersAsync(string query, int page = 1, int pageSize = 20)
        {
            return _profileQueries.SearchUsersAsync(query, page, pageSize);
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
