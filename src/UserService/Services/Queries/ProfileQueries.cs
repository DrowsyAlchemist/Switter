using AutoMapper;
using UserService.Interfaces.Data;
using UserService.Interfaces;
using UserService.Interfaces.Queries;
using UserService.DTOs;
using UserService.Exceptions.Profiles;

namespace UserService.Services.Queries
{
    public class ProfileQueries : IProfileQueries
    {
        private readonly IProfilesRepository _profilesRepository;
        private readonly IMapper _mapper;

        public ProfileQueries(IProfilesRepository profilesRepository, IUserRelationshipService relationshipService, IMapper mapper)
        {
            _profilesRepository = profilesRepository;
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
            return userProfileDto;
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
