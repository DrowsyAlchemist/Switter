using AutoMapper;
using System.Text.Json;
using UserService.DTOs;
using UserService.Exceptions.Profiles;
using UserService.Interfaces;
using UserService.Interfaces.Data;
using UserService.Interfaces.Infrastructure;

namespace UserService.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IProfilesRepository _profilesRepository;
        private readonly IFollowChecker _followChecker;
        private readonly IRedisService _redisService;
        private readonly IMapper _mapper;
        private readonly ILogger<UserProfileService> _logger;

        public UserProfileService(IProfilesRepository profilesRepository, IFollowChecker followChecker, IMapper mapper,
                                IRedisService redisService, ILogger<UserProfileService> logger)
        {
            _profilesRepository = profilesRepository;
            _followChecker = followChecker;
            _mapper = mapper;
            _redisService = redisService;
            _logger = logger;
        }

        public async Task<UserProfileDto> GetProfileAsync(Guid userId, Guid? currentUserId = null)
        {
            var cacheKey = GetRedisKey(userId);
            UserProfileDto? profileDto = await GetProfileFromCacheAsync(cacheKey);

            if (profileDto != null)
            {
                if (currentUserId.HasValue)
                    profileDto.IsFollowing = await _followChecker.IsFollowingAsync(currentUserId.Value, userId);

                return profileDto;
            }

            var profileFromDb = await _profilesRepository.GetProfileAsync(userId);
            if (profileFromDb == null)
                throw new UserNotFoundException(userId);
            if (profileFromDb.IsActive == false)
                throw new UserDeactivatedException(userId);

            profileDto = _mapper.Map<UserProfileDto>(profileFromDb);

            if (currentUserId.HasValue)
                profileDto.IsFollowing = await _followChecker.IsFollowingAsync(currentUserId.Value, userId);

            await _redisService.SetAsync(cacheKey,
                JsonSerializer.Serialize(profileDto),
                TimeSpan.FromMinutes(5));

            return profileDto;
        }

        public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
        {
            var profile = await _profilesRepository.GetProfileAsync(userId);
            if (profile == null)
                throw new UserNotFoundException(userId);
            if (profile.IsActive == false)
                throw new UserDeactivatedException(userId);

            if (string.IsNullOrEmpty(request.DisplayName) == false)
                profile.DisplayName = request.DisplayName;

            if (request.Bio != null)
                profile.Bio = request.Bio;

            if (string.IsNullOrEmpty(request.AvatarUrl) == false)
                profile.AvatarUrl = request.AvatarUrl;

            profile.UpdatedAt = DateTime.UtcNow;

            await _profilesRepository.UpdateProfileAsync(profile);

            var redisKey = GetRedisKey(userId);
            await _redisService.RemoveAsync(redisKey);

            return _mapper.Map<UserProfileDto>(profile);
        }

        public async Task<List<UserProfileDto>> SearchUsersAsync(string query, int page = 1, int pageSize = 20)
        {
            var users = await _profilesRepository.GetUsersAsync();
            users = users
                .Where(p => p.IsActive
                && (p.DisplayName.Contains(query) || p.Bio.Contains(query)))
                .OrderBy(p => p.DisplayName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return _mapper.Map<List<UserProfileDto>>(users);
        }

        private static string GetRedisKey(Guid userId) => $"profile:{userId}";

        private async Task<UserProfileDto?> GetProfileFromCacheAsync(string key)
        {

            var cachedProfile = await _redisService.GetAsync(key);
            UserProfileDto? profileDto = null;

            if (string.IsNullOrEmpty(cachedProfile) == false)
            {
                try
                {
                    profileDto = JsonSerializer.Deserialize<UserProfileDto>(cachedProfile);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Cached data is invalid.\nKey: {key}\nCachedProfile: {cache}", key, cachedProfile);
                }
            }
            return profileDto;
        }
    }
}
