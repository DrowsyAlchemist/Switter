using UserService.DTOs;

namespace UserService.Interfaces.Queries
{
    public interface IFollowQueries
    {
        Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId);
        Task<IEnumerable<UserProfileDto>> GetFollowersAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<IEnumerable<UserProfileDto>> GetFollowingsAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<IEnumerable<Guid>> GetFollowerIdsAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<IEnumerable<Guid>> GetFollowingIdsAsync(Guid userId, int page = 1, int pageSize = 20);
    }
}
