using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UserService.Data;
using UserService.DTOs;
using UserService.Interfaces;
using UserService.Interfaces.Infrastructure;

namespace UserService.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly UserDbContext _context;
        private readonly IMapper _mapper;
        private readonly IRedisService _redisService;
        private readonly ILogger<UserProfileService> _logger;

        public UserProfileService(UserDbContext context, IMapper mapper,
                                IRedisService redisService, ILogger<UserProfileService> logger)
        {
            _context = context;
            _mapper = mapper;
            _redisService = redisService;
            _logger = logger;
        }

        public async Task<UserProfileDto?> GetProfileAsync(Guid userId, Guid? currentUserId = null)
        {
            var cacheKey = GetRedisKey(userId);
            var cachedProfile = await _redisService.GetAsync(cacheKey);
            UserProfileDto? profileDto = null;

            if (string.IsNullOrEmpty(cachedProfile) == false)
            {
                profileDto = JsonSerializer.Deserialize<UserProfileDto>(cachedProfile);

                if (profileDto != null)
                {
                    if (currentUserId.HasValue)
                        profileDto.IsFollowing = await IsFollowingAsync(currentUserId.Value, userId);

                    return profileDto;
                }
            }

            var userProfile = await _context.Profiles.FirstOrDefaultAsync(p => p.Id == userId && p.IsActive);

            if (userProfile == null)
                return null;

            profileDto = _mapper.Map<UserProfileDto>(userProfile);

            if (currentUserId.HasValue)
                profileDto.IsFollowing = await IsFollowingAsync(currentUserId.Value, userId);

            await _redisService.SetAsync(cacheKey,
                JsonSerializer.Serialize(profileDto),
                TimeSpan.FromMinutes(5));

            return profileDto;
        }

        public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
        {
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.Id == userId && p.IsActive);

            if (profile == null)
                throw new ArgumentException("User profile not found");

            if (string.IsNullOrEmpty(request.DisplayName) == false)
                profile.DisplayName = request.DisplayName;

            if (request.Bio != null)
                profile.Bio = request.Bio;

            if (string.IsNullOrEmpty(request.AvatarUrl) == false)
                profile.AvatarUrl = request.AvatarUrl;

            profile.UpdatedAt = DateTime.UtcNow;

            _context.Profiles.Update(profile);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Profile updated for user {UserId}", userId);

            var redisKey = GetRedisKey(userId);
            await _redisService.RemoveAsync(redisKey);

            return _mapper.Map<UserProfileDto>(profile);
        }

        public async Task<List<UserProfileDto>> SearchUsersAsync(string query, int page = 1, int pageSize = 20)
        {
            throw new NotImplementedException();
        }

        private static string GetRedisKey(Guid userId)
        {
            return $"profile:{userId}";
        }

        private async Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId)
        {
            return await _context.Follows
                .AnyAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);
        }
    }
}
