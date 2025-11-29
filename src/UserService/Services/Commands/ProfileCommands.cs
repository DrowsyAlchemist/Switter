using AutoMapper;
using UserService.DTOs;
using UserService.Exceptions.Profiles;
using UserService.Interfaces.Commands;
using UserService.Interfaces.Data;

namespace UserService.Services.Commands
{
    public class ProfileCommands : IProfileCommands
    {
        private readonly IProfilesRepository _profilesRepository;
        private readonly IMapper _mapper;

        public ProfileCommands(IProfilesRepository profilesRepository, IMapper mapper)
        {
            _profilesRepository = profilesRepository;
            _mapper = mapper;
        }

        public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
        {
            var profile = await _profilesRepository.GetProfileByIdAsync(userId);
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
    }
}
