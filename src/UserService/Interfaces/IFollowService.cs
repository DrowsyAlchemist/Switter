using UserService.DTOs;

namespace UserService.Interfaces
{
    public interface IFollowService
    {
        Task<bool> FollowUserAsync(Guid followerId, Guid followeeId);
        Task<bool> UnfollowUserAsync(Guid followerId, Guid followeeId);
        Task<List<UserProfileDto>> GetFollowersAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<List<UserProfileDto>> GetFollowingAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId);
    }
}
