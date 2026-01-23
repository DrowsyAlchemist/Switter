using AutoMapper;
using TweetService.DTOs;
using TweetService.Exceptions;
using TweetService.Interfaces.Data;
using TweetService.Interfaces.Services;

namespace TweetService.Services
{
    public class TweetQueries : ITweetQueries
    {
        private readonly ITweetRepository _tweetRepository;
        private readonly IUserTweetRelationship _userTweetRelationship;
        private readonly IMapper _mapper;

        public TweetQueries(ITweetRepository tweetRepository,
            IUserTweetRelationship userTweetRelationship,
            IMapper mapper)
        {
            _tweetRepository = tweetRepository;
            _userTweetRelationship = userTweetRelationship;
            _mapper = mapper;
        }

        public async Task<TweetDto> GetTweetAsync(Guid tweetId, Guid? currentUserId = null)
        {
            var tweet = await _tweetRepository.GetByIdAsync(tweetId);
            if (tweet == null)
                throw new TweetNotFoundException(tweetId);

            var tweetDto = _mapper.Map<TweetDto>(tweet);

            if (currentUserId.HasValue)
                tweetDto = await _userTweetRelationship.GetTweetWithRelationshipsAsync(tweetDto, currentUserId.Value);

            return tweetDto;
        }

        public async Task<IEnumerable<TweetDto>> GetTweetRepliesAsync(Guid tweetId, int page, int pageSize, Guid? currentUserId = null)
        {
            var replies = await _tweetRepository.GetRepliesAsync(tweetId, page, pageSize);
            var tweetDtos = _mapper.Map<IEnumerable<TweetDto>>(replies);

            if (currentUserId.HasValue)
                tweetDtos = await _userTweetRelationship.GetTweetsWithRelationshipsAsync(tweetDtos, currentUserId.Value);

            return tweetDtos;
        }

        public async Task<IEnumerable<TweetDto>> GetUserTweetsAsync(Guid userId, int page, int pageSize, Guid? currentUserId = null)
        {
            var tweets = await _tweetRepository.GetByUserAsync(userId, page, pageSize);
            var tweetDtos = _mapper.Map<IEnumerable<TweetDto>>(tweets);

            if (currentUserId.HasValue)
                tweetDtos = await _userTweetRelationship.GetTweetsWithRelationshipsAsync(tweetDtos, currentUserId.Value);

            return tweetDtos;
        }
    }
}
