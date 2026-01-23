using TweetService.Interfaces.Data;
using TweetService.Interfaces.Services;

namespace TweetService.Services.Decorators
{
    public class HashtagServiceWithTrendFiller : IHashtagService
    {
        private readonly TrendFiller _trendFiller;
        private readonly IHashtagService _hashtagService;
        private readonly ITransactionManager _transactionManager;

        public HashtagServiceWithTrendFiller(IHashtagService hashtagService, TrendFiller trendFiller, ITransactionManager transactionManager)
        {
            _trendFiller = trendFiller;
            _hashtagService = hashtagService;
            _transactionManager = transactionManager;
        }

        public async Task<IEnumerable<string>> ProcessHashtagsAsync(Guid tweetId, string content)
        {
            await using var transaction = await _transactionManager.BeginTransactionAsync();
            try
            {
                var hashtags = await _hashtagService.ProcessHashtagsAsync(tweetId, content);
                if (hashtags.Any() == false)
                    return [];

                await _trendFiller.SetHashtagsUsageAsync(hashtags);
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
