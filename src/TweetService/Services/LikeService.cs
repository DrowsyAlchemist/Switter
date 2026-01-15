using AutoMapper;
using TweetService.DTOs;
using TweetService.Exceptions;
using TweetService.Interfaces.Data;
using TweetService.Interfaces.Services;
using TweetService.Models;

namespace TweetService.Services
{
    public class LikeService : ILikeService
    {
        private readonly ILikesRepository _likesRepository;
        private readonly ITweetRepository _tweetRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IMapper _mapper;

        public LikeService(ILikesRepository likesRepository, ITweetRepository tweetRepository, IMapper mapper, ITransactionManager transactionManager)
        {
            _likesRepository = likesRepository;
            _tweetRepository = tweetRepository;
            _transactionManager = transactionManager;
            _mapper = mapper;
        }

        public async Task<List<TweetDto>> GetLikedTweetsAsync(Guid userId, int page, int pageSize)
        {
            var likedTweetIds = await _likesRepository.GetLikedTweetIdsAsync(userId, page, pageSize);
            var likedTweets = await _tweetRepository.GetByIdsAsync(likedTweetIds, page, pageSize);
            var likedTweetsDtos = _mapper.Map<List<TweetDto>>(likedTweets);

            var retweetedIds = await _tweetRepository.GetRetweetedIdsAsync(likedTweetIds, userId);
            foreach (var tweetDto in likedTweetsDtos)
            {
                tweetDto.IsLiked = true;
                tweetDto.IsRetweeted = retweetedIds.Contains(tweetDto.Id);
            }
            return likedTweetsDtos;
        }

        public async Task LikeTweetAsync(Guid tweetId, Guid userId)
        {
            await using var transaction = await _transactionManager.BeginTransactionAsync();
            try
            {
                bool isLikeExist = await _likesRepository.IsExistAsync(tweetId, userId);
                if (isLikeExist)
                    throw new DoubleLikeException(tweetId, userId);

                var tweet = await _tweetRepository.GetByIdAsync(tweetId);
                if (tweet == null)
                    throw new TweetNotFoundException(tweetId);

                var like = new Like()
                {
                    TweetId = tweetId,
                    UserId = userId
                };
                var likeInDb = await _likesRepository.AddAsync(like);
                await _tweetRepository.IncrementLikesCount(tweet.Id);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UnlikeTweetAsync(Guid tweetId, Guid userId)
        {
            await using var transaction = await _transactionManager.BeginTransactionAsync();
            try
            {
                var like = await _likesRepository.GetAsync(tweetId, userId);
                if (like == null)
                    throw new LikeNotFoundException(tweetId, userId);

                var tweet = await _tweetRepository.GetByIdAsync(tweetId);
                if (tweet == null)
                    throw new TweetNotFoundException(tweetId);

                await _likesRepository.DeleteAsync(like.Id);
                await _tweetRepository.DecrementLikesCount(tweet.Id);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}