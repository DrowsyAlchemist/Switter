using AutoMapper;
using UserService.DTOs;
using UserService.Exceptions.Profiles;
using UserService.Interfaces;
using UserService.Interfaces.Data;

namespace UserService.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IProfilesRepository _profilesRepository;
        private readonly IUserRelationshipService _relationshipService;
        private readonly IMapper _mapper;

        public UserProfileService(IProfilesRepository profilesRepository, IUserRelationshipService relationshipService, IMapper mapper)
        {
            _profilesRepository = profilesRepository;
            _relationshipService = relationshipService;
            _mapper = mapper;
        }

        public async Task<UserProfileDto> GetProfileAsync(Guid userId, Guid? currentUserId = null)
        {
            var profile = await _profilesRepository.GetProfileAsync(userId);

            if (profile == null)
                throw new UserNotFoundException(userId);
            if (profile.IsActive == false)
                throw new UserDeactivatedException(userId);

            var userProfileDto = _mapper.Map<UserProfileDto>(profile);

            if (currentUserId.HasValue)
            {
                userProfileDto = await _relationshipService.GetProfileWithUserRelationInfoAsync(userProfileDto, currentUserId.Value);
                if (userProfileDto.IsBlocking)
                    throw new GetBlockerException();
            }
            return userProfileDto;
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

            return _mapper.Map<UserProfileDto>(profile);
        }

        public async Task<List<UserProfileDto>> SearchUsersAsync(string? query, int page = 1, int pageSize = 20)
        {
            if (query == null)
                query = string.Empty;

            query = query.ToLower();

            var users = await _profilesRepository.GetUsersAsync();
            users = users
                .Where(p => p.IsActive
                && (p.DisplayName.ToLower().Contains(query) || p.Bio.ToLower().Contains(query)))
                .OrderBy(p => p.DisplayName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return _mapper.Map<List<UserProfileDto>>(users);
        }
    }
}
