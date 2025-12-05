using TweetService.DTOs;

namespace TweetService.Interfaces.Services
{
    public interface IHashtagService
    {
        Task<List<string>> ExtractHashtagsAsync(string content);
        Task ProcessHashtagsAsync(Guid tweetId, string content);
        Task<List<HashtagTrendDto>> GetTrendingHashtagsAsync(int count = 10);
    }
}
