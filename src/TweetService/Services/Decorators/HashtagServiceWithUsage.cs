using TweetService.Interfaces.Data;
using TweetService.Interfaces.Infrastructure;
using TweetService.Interfaces.Services;

namespace TweetService.Services.Decorators
{
    public class HashtagServiceWithUsage : IHashtagService
    {
        private const string KeyForHashtagUsage = "KeyForHashtagUsages";
        private readonly IHashtagService _hashtagService;
        private readonly IRedisService _redisService;
        private readonly ITransactionManager _transactionManager;

        public HashtagServiceWithUsage(IHashtagService hashtagService, IRedisService redisService, ITransactionManager transactionManager)
        {
            _hashtagService = hashtagService;
            _redisService = redisService;
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

                await _redisService.AddToListAsync(KeyForHashtagUsage, hashtags);
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
