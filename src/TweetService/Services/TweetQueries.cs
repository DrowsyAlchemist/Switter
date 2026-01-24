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
        private readonly IMapper _mapper;

        public TweetQueries(ITweetRepository tweetRepository,
            IMapper mapper)
        {
            _tweetRepository = tweetRepository;
            _mapper = mapper;
        }

        public async Task<TweetDto> GetTweetAsync(Guid tweetId, Guid? currentUserId = null)
        {
            var tweet = await _tweetRepository.GetByIdAsync(tweetId);
            if (tweet == null)
                throw new TweetNotFoundException(tweetId);

            return _mapper.Map<TweetDto>(tweet);
        }

        public async Task<IEnumerable<TweetDto>> GetTweetRepliesAsync(Guid tweetId, int page, int pageSize, Guid? currentUserId = null)
        {
            var replies = await _tweetRepository.GetRepliesAsync(tweetId, page, pageSize);
            var tweetDtos = _mapper.Map<IEnumerable<TweetDto>>(replies);
            return tweetDtos;
        }

        public async Task<IEnumerable<TweetDto>> GetUserTweetsAsync(Guid userId, int page, int pageSize, Guid? currentUserId = null)
        {
            var tweets = await _tweetRepository.GetByUserAsync(userId, page, pageSize);
            var tweetDtos = _mapper.Map<IEnumerable<TweetDto>>(tweets);
            return tweetDtos;
        }
    }
}
