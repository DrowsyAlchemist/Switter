using TweetService.DTOs;

namespace TweetService.Interfaces.Services
{
    public interface IUserTweetRelationship
    {
        Task<TweetDto> GetTweetWithRelationshipsAsync(TweetDto tweetDto, Guid userId);
        Task<List<TweetDto>> GetTweetsWithRelationshipsAsync(List<TweetDto> tweetDtos, Guid userId);
    }
}