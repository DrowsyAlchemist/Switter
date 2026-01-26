using TweetService.DTOs;
using TweetService.Interfaces.Data;
using TweetService.Interfaces.Services;

namespace TweetService.Services.Decorators.Transactions
{
    public class TweetCommandsWithTransaction : ITweetCommands
    {
        private readonly ITweetCommands _tweetCommands;
        private readonly ITransactionManager _transactionManager;

        public TweetCommandsWithTransaction(ITweetCommands tweetCommands, ITransactionManager transactionManager)
        {
            _tweetCommands = tweetCommands;
            _transactionManager = transactionManager;
        }

        public async Task<TweetDto> TweetAsync(UserInfo authorInfo, CreateTweetRequest request)
        {
            await using var transaction = await _transactionManager.BeginTransactionAsync();

            try
            {
                var tweetDto = await _tweetCommands.TweetAsync(authorInfo, request);
                await transaction.CommitAsync();
                return tweetDto;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteTweetAsync(Guid tweetId, Guid userId)
        {
            await using var transaction = await _transactionManager.BeginTransactionAsync();

            try
            {
                await _tweetCommands.DeleteTweetAsync(tweetId, userId);
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
