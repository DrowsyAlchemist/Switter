using TweetService.Models;

namespace TweetService.Interfaces.Data
{
    public interface ILikesRepository
    {
        Task<Like?> GetById(Guid id);
        Task<List<Like>> GetByUser(Guid userId);
        Task<Like> Add(Like like);
        Task<Like> Delete(Guid id);
    }
}
