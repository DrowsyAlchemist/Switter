using TweetService.Models;

namespace TweetService.Interfaces.Data
{
    public interface IHashtagRepository
    {
        Task<Hashtag?> GetById(Guid id);
        Task<List<Hashtag>> Search(string query, int page, int pageSize);
        Task<Hashtag> Add(Hashtag hashtag);
        Task<Hashtag> Update(Hashtag hashtag);
        Task<Hashtag> Delete(Guid id);
    }
}
