using TweetService.Models;

namespace TweetService.Interfaces.Data
{
    public interface ILikesRepository
    {
        Task<Like?> GetByIdAsync(Guid id);
        Task<List<Like>> GetByUserAsync(Guid userId);
        Task<Like> AddAsync(Like like);
        Task<Like> DeleteAsync(Guid id);
        Task<bool> IsExist(Guid userId, Guid tweetId);
    }
}
