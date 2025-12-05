using TweetService.DTOs;

namespace TweetService.Interfaces.Services
{
    public interface IUserTweetRelationship
    {
        Task<TweetDto> GetTweetWithRelationshipsAsync(TweetDto tweetDto, Guid userId);
    }
}