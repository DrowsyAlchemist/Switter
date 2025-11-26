using TweetService.Models;

namespace TweetService.Interfaces.Data
{
    public interface ITweetRepository
    {
        Task<Tweet?> GetById(Guid id);
        Task<List<Tweet>> GetByUser(Guid userId);
        Task<Tweet> Add(Tweet tweet);
        Task<Tweet> Update(Tweet tweet);
        Task<Tweet> Delete(Guid id);
    }
}
