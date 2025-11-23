using UserService.DTOs;
using UserService.Exceptions.Profiles;
using UserService.Interfaces;
using UserService.Interfaces.Queries;

namespace UserService.Services.Decorators
{
    public class ProfileQueriesWithRelationship : IProfileQueries
    {
        private readonly IProfileQueries _profileQueries;
        private readonly IUserRelationshipService _userRelationshipService;

        public ProfileQueriesWithRelationship(IProfileQueries profileQueries, IUserRelationshipService userRelationshipService)
        {
            _profileQueries = profileQueries;
            _userRelationshipService = userRelationshipService;
        }

        public async Task<UserProfileDto> GetProfileAsync(Guid userId, Guid? currentUserId = null)
        {
            var profileDto = await _profileQueries.GetProfileAsync(userId, currentUserId);

            if (currentUserId.HasValue)
            {
                profileDto = await _userRelationshipService.GetProfileWithUserRelationInfoAsync(profileDto, currentUserId.Value);
                if (profileDto.IsBlocking)
                    throw new GetBlockerException();
            }
            return profileDto;
        }

        public async Task<List<UserProfileDto>> SearchUsersAsync(string query, int page = 1, int pageSize = 20)
        {
            return await _profileQueries.SearchUsersAsync(query, page, pageSize);
        }
    }
}
