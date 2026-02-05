using UserService.Models;

namespace UserService.Interfaces.Data
{
    public interface IFollowRepository
    {
        Task<Follow> AddAsync(Guid followerId, Guid followeeId);
        Task DeleteAsync(Guid followerId, Guid followeeId);

        Task<Follow?> GetAsync(Guid followerId, Guid followeeId);
        Task<IEnumerable<UserProfile>> GetFollowersAsync(Guid followeeId, int page, int pageSize);
        Task<IEnumerable<UserProfile>> GetFollowingsAsync(Guid followerId, int page, int pageSize);
        Task<IEnumerable<Guid>> GetFollowerIdsAsync(Guid followeeId, int page, int pageSize);
        Task<IEnumerable<Guid>> GetFollowingIdsAsync(Guid followerId, int page, int pageSize);
        Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId);
    }
}
