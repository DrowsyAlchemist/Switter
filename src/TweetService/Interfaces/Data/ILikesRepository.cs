using TweetService.Models;

namespace TweetService.Interfaces.Data
{
    public interface ILikesRepository
    {
        Task<Like?> GetByIdAsync(Guid id);
        Task<List<Like>> GetByUserAsync(Guid userId);
        Task<List<Guid>> GetLikedTweetIdsAsync(List<Guid> tweetIds, Guid userId);
        Task<List<Guid>> GetLikedTweetIdsAsync(Guid userId, int page, int pageSize);
        Task<Like?> GetAsync(Guid tweetId, Guid userId);
        Task<Like> AddAsync(Like like);
        Task DeleteAsync(Guid id);
        Task<bool> IsExistAsync(Guid tweetId, Guid userId);
    }
}
