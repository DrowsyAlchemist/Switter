using TweetService.DTOs;

namespace TweetService.Interfaces.Services
{
    public interface IUserTweetRelationshipService
    {
        Task<TweetDto> GetTweetWithRelationshipsAsync(TweetDto tweetDto, Guid userId);
        Task<IEnumerable<TweetDto>> GetTweetsWithRelationshipsAsync(IEnumerable<TweetDto> tweetDtos, Guid userId);
    }
}