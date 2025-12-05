using TweetService.DTOs;
using TweetService.Models;

namespace TweetService.Interfaces.Services
{
    public interface ITrendService
    {
        Task<List<TweetDto>> GetTrends(int count = 10);
        Task<List<TweetDto>> GetTrends(string hashtag, int count = 10);
    }
}
