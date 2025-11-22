using UserService.DTOs;

namespace UserService.Interfaces
{
    public interface IUserRelationshipService
    {
        Task<UserProfileDto> GetProfileWithUserRelationInfoAsync(UserProfileDto profile, Guid userId);
        Task<bool> IsBlockedAsync(Guid blockerId, Guid blockedId);
        Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId);
    }
}
