using TweetService.DTOs;

namespace TweetService.Interfaces.Services
{
    public interface ITweetCommands
    {
        Task<TweetDto> TweetAsync(Guid authorId, CreateTweetRequest request);
        Task<TweetDto> DeleteTweetAsync(Guid tweetId, Guid userId);
    }
}
