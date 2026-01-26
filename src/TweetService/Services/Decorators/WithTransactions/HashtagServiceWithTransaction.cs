using TweetService.Interfaces.Data;
using TweetService.Interfaces.Services;

namespace TweetService.Services.Decorators.WithTransactions
{
    public class HashtagServiceWithTransaction : IHashtagService
    {
        private readonly IHashtagService _hashtagService;
        private readonly ITransactionManager _transactionManager;

        public HashtagServiceWithTransaction(IHashtagService hashtagService, ITransactionManager transactionManager)
        {
            _hashtagService = hashtagService;
            _transactionManager = transactionManager;
        }

        public async Task<IEnumerable<string>> ProcessHashtagsAsync(Guid tweetId, string content)
        {
            await using var transaction = await _transactionManager.BeginTransactionAsync();

            try
            {
                var hashtags = await _hashtagService.ProcessHashtagsAsync(tweetId, content);
                await transaction.CommitAsync();
                return hashtags;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
