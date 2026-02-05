using AutoMapper;
using TweetService.DTOs;
using TweetService.Exceptions;
using TweetService.Interfaces.Data.Repositories;
using TweetService.Interfaces.Services;

namespace TweetService.Services.Tweets
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

        public async Task<TweetDto> GetTweetAsync(Guid tweetId)
        {
            var tweet = await _tweetRepository.GetByIdAsync(tweetId);
            if (tweet == null)
                throw new TweetNotFoundException(tweetId);

            return _mapper.Map<TweetDto>(tweet);
        }

        public async Task<List<TweetDto>> GetTweetsAsync(List<Guid> tweetIds)
        {
            var tweets = await _tweetRepository.GetByIdsAsync(tweetIds);
            return _mapper.Map<List<TweetDto>>(tweets);
        }

        public async Task<List<TweetDto>> GetTweetRepliesAsync(Guid tweetId, int page, int pageSize)
        {
            var replies = await _tweetRepository.GetRepliesAsync(tweetId, page, pageSize);
            var tweetDtos = _mapper.Map<List<TweetDto>>(replies);
            return tweetDtos;
        }

        public async Task<List<TweetDto>> GetUserTweetsAsync(Guid userId, int page, int pageSize)
        {
            var tweets = await _tweetRepository.GetByUserAsync(userId, page, pageSize);
            var tweetDtos = _mapper.Map<List<TweetDto>>(tweets);
            return tweetDtos;
        }

        public async Task<List<Guid>> GetUserTweetIdsAsync(Guid userId, int page, int pageSize)
        {
            return await _tweetRepository.GetIdsByUserAsync(userId, page, pageSize);
        }
    }
}
