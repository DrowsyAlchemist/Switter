using TweetService.DTOs;
using TweetService.Interfaces.Data;
using TweetService.Interfaces.Services;

namespace TweetService.Services.Decorators.WithTransactions
{
    public class LikeServiceWithTransaction : ILikeService
    {
        private readonly ILikeService _likeService;
        private readonly ITransactionManager _transactionManager;

        public LikeServiceWithTransaction(ILikeService likeService, ITransactionManager transactionManager)
        {
            _likeService = likeService;
            _transactionManager = transactionManager;
        }

        public async Task LikeTweetAsync(Guid tweetId, Guid userId)
        {
            await using var transaction = await _transactionManager.BeginTransactionAsync();

            try
            {
                await _likeService.LikeTweetAsync(tweetId, userId);
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
                await _likeService.UnlikeTweetAsync(tweetId, userId);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public Task<List<TweetDto>> GetLikedTweetsAsync(Guid userId, int page, int pageSize)
        {
            return _likeService.GetLikedTweetsAsync(userId, page, pageSize);
        }
    }
}
