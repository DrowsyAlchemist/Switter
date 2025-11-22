using UserService.DTOs;
using UserService.Interfaces;
using UserService.Interfaces.Data;

namespace UserService.Services
{
    public class ProfileRelationshipService : IUserRelationshipService
    {
        private readonly IUserProfileService _profileService;
        private readonly IFollowRepository _followRepository;
        private readonly IBlockRepository _blockRepository;

        public ProfileRelationshipService(IUserProfileService profileService, IFollowRepository followRepository, IBlockRepository blockRepository)
        {
            _profileService = profileService;
            _followRepository = followRepository;
            _blockRepository = blockRepository;
        }

        public async Task<UserProfileDto> GetProfileWithUserRelationInfoAsync(UserProfileDto profile, Guid userId)
        {
            profile.IsFollowed = await _followRepository.IsFollowingAsync(userId, profile.Id);
            profile.IsFollowing = await _followRepository.IsFollowingAsync(profile.Id, userId);
            profile.IsBlocked = await _blockRepository.IsBlockedAsync(userId, profile.Id);
            profile.IsBlocking = await _blockRepository.IsBlockedAsync(profile.Id, userId);
            return profile;
        }

        public async Task<bool> IsBlockedAsync(Guid blockerId, Guid blockedId)
        {
            return await _blockRepository.IsBlockedAsync(blockedId, blockerId);
        }

        public async Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId)
        {
            return await _followRepository.IsFollowingAsync(followerId, followeeId);
        }
    }
}