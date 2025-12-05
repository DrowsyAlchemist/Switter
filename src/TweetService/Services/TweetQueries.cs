using AutoMapper;
using TweetService.DTOs;
using TweetService.Exceptions;
using TweetService.Interfaces.Data;
using TweetService.Interfaces.Infrastructure;
using TweetService.Interfaces.Services;

namespace TweetService.Services
{
    public class TweetQueries : ITweetQueries
    {
        private readonly ITweetRepository _tweetRepository;
        private readonly IUserServiceClient _userServiceClient;
        private readonly IUserTweetRelationship _userTweetRelationship;
        private readonly IMapper _mapper;

        public TweetQueries(ITweetRepository tweetRepository,
            IUserServiceClient userServiceClient,
            IUserTweetRelationship userTweetRelationship,
            IMapper mapper)
        {
            _tweetRepository = tweetRepository;
            _userServiceClient = userServiceClient;
            _userTweetRelationship = userTweetRelationship;
            _mapper = mapper;
        }

        public async Task<TweetDto?> GetTweetAsync(Guid tweetId, Guid? currentUserId = null)
        {
            var tweet = await _tweetRepository.GetByIdAsync(tweetId);
            if (tweet == null)
                throw new TweetNotFoundException(tweetId);

            var tweetDto = _mapper.Map<TweetDto>(tweet);

            if (currentUserId.HasValue)
                tweetDto = await _userTweetRelationship.GetTweetWithRelationshipsAsync(tweetDto, currentUserId.Value);

            return tweetDto;
        }

        public async Task<List<TweetDto>> GetTweetRepliesAsync(Guid tweetId, int page = 1, int pageSize = 20, Guid? currentUserId = null)
        {
            var replies = await _tweetRepository.GetRepliesAsync(tweetId);
            replies = replies.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var tweetDtos = _mapper.Map<List<TweetDto>>(replies);

            if (currentUserId.HasValue)
            {
                var repliesWithRelationship = new List<TweetDto>();
                foreach (var tweet in tweetDtos)
                {
                    var tweetWithRelationships = await _userTweetRelationship.GetTweetWithRelationshipsAsync(tweet, currentUserId.Value);
                    repliesWithRelationship.Add(tweetWithRelationships);
                }
                tweetDtos = repliesWithRelationship;
            }
            return tweetDtos;
        }

        public async Task<List<TweetDto>> GetUserTweetsAsync(Guid userId, int page = 1, int pageSize = 20, Guid? currentUserId = null)
        {
            var tweets = await _tweetRepository.GetByUserAsync(userId);
            tweets = tweets.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var tweetDtos = _mapper.Map<List<TweetDto>>(tweets);

            if (currentUserId.HasValue)
            {
                var repliesWithRelationship = new List<TweetDto>();
                foreach (var tweet in tweetDtos)
                {
                    var tweetWithRelationships = await _userTweetRelationship.GetTweetWithRelationshipsAsync(tweet, currentUserId.Value);
                    repliesWithRelationship.Add(tweetWithRelationships);
                }
                tweetDtos = repliesWithRelationship;
            }
            return tweetDtos;
        }
    }
}
