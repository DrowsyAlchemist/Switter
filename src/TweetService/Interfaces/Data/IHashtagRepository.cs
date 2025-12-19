using TweetService.Models;

namespace TweetService.Interfaces.Data
{
    public interface IHashtagRepository
    {
        Task<Hashtag?> GetByTagAsync(string tag);
        Task<List<Hashtag>> GetByTagsAsync(List<string> tags);
        Task<List<Hashtag>> SearchAsync(string query, int page, int pageSize);
        Task<List<Hashtag>> GetMostPopularAsync(int count);
        Task<Hashtag> AddAsync(Hashtag hashtag);
        Task<Hashtag> UpdateAsync(Hashtag hashtag);
        Task<Hashtag> DeleteAsync(Guid id);
    }
}
