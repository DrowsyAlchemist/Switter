using TweetService.DTOs;
using TweetService.Interfaces.Data;
using TweetService.Interfaces.Services;

namespace TweetService.Services
{
    public class UserTweetRelationshipService : IUserTweetRelationshipService
    {
        private readonly ITweetRepository _tweetRepository;
        private readonly ILikesRepository _likesRepository;

        public UserTweetRelationshipService(ITweetRepository tweetRepository, ILikesRepository likesRepository)
        {
            _tweetRepository = tweetRepository;
            _likesRepository = likesRepository;
        }

        public async Task<TweetDto> GetTweetWithRelationshipsAsync(TweetDto tweetDto, Guid userId)
        {
            tweetDto.IsLiked = await _likesRepository.IsExistAsync(tweetDto.Id, userId);
            tweetDto.IsRetweeted = await _tweetRepository.IsRetweetedAsync(tweetDto.Id, userId);
            return tweetDto;
        }

        public async Task<IEnumerable<TweetDto>> GetTweetsWithRelationshipsAsync(IEnumerable<TweetDto> tweetDtos, Guid userId)
        {
            ArgumentNullException.ThrowIfNull(tweetDtos);
            if (tweetDtos.Any() == false)
                return tweetDtos;

            var tweetIds = tweetDtos.Select(t => t.Id).ToList();

            var likedTweetIds = await _likesRepository.GetLikedTweetIdsAsync(tweetIds, userId);
            var retweetedIds = await _tweetRepository.GetRetweetedIdsAsync(tweetIds, userId);

            foreach (var tweetDto in tweetDtos)
            {
                tweetDto.IsLiked = likedTweetIds.Contains(tweetDto.Id);
                tweetDto.IsRetweeted = retweetedIds.Contains(tweetDto.Id);
            }
            return tweetDtos;
        }
    }
}
