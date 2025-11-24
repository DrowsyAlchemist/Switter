using UserService.Models;

namespace UserService.Interfaces.Data
{
    public interface IFollowRepository
    {
        Task<Follow> AddAsync(Guid followerId, Guid followeeId);
        Task<Follow?> GetAsync(Guid followerId, Guid followeeId);
        Task DeleteAsync(Guid followerId, Guid followeeId);
        Task<List<UserProfile>> GetFollowersAsync(Guid followeeId);
        Task<List<UserProfile>> GetFollowingsAsync(Guid followerId);
        Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId);
    }
}
