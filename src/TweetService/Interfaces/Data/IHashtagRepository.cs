using TweetService.Models;

namespace TweetService.Interfaces.Data
{
    public interface IHashtagRepository
    {
        Task<Hashtag?> GetByIdAsync(Guid id);
        Task<List<Hashtag>> SearchAsync(string query, int page, int pageSize);
        Task<Hashtag> AddAsync(Hashtag hashtag);
        Task<Hashtag> UpdateAsync(Hashtag hashtag);
        Task<Hashtag> DeleteAsync(Guid id);
    }
}
