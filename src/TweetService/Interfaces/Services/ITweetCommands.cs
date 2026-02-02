using TweetService.DTOs;

namespace TweetService.Interfaces.Services
{
    public interface ITweetCommands
    {
        Task<TweetDto> TweetAsync(UserInfo authorInfo, CreateTweetRequest request);
        Task DeleteTweetAsync(Guid tweetId, Guid userId);
    }
}
