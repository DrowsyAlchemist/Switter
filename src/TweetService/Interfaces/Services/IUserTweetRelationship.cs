using TweetService.DTOs;

namespace TweetService.Interfaces.Services
{
    public interface IUserTweetRelationship
    {
        Task<TweetDto> GetTweetWithRelationshipsAsync(TweetDto tweetDto, Guid userId);
        Task<IEnumerable<TweetDto>> GetTweetsWithRelationshipsAsync(IEnumerable<TweetDto> tweetDtos, Guid userId);
    }
}