using FluentAssertions.Common;
using StackExchange.Redis;
using TweetService.Interfaces.Infrastructure;

namespace TweetService.Services.Infrastructure
{
    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger _logger;

        public RedisService(IConnectionMultiplexer redis, ILogger<RedisService> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        public async Task AddToListAsync(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            try
            {
                long timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var db = _redis.GetDatabase();
                await db.SortedSetAddAsync(key, value, timeStamp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis is unavailable");
                throw new Exception("Redis is unavailable" + ex);
            }
        }

        public async Task<List<string>> GetListFromDateAsync(string key, DateTime startDateTime)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            try
            {
                long startTimeStamp = startDateTime.ToDateTimeOffset().ToUnixTimeSeconds();
                var db = _redis.GetDatabase();
                var list = await db.SortedSetRangeByRankWithScoresAsync(key, order: Order.Descending, start: startTimeStamp);
                return list.Select(x => x.ToString()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis is unavailable");
                throw new Exception("Redis is unavailable" + ex);
            }
        }

        public async Task<string?> GetAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            try
            {
                var db = _redis.GetDatabase();
                return await db.StringGetAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis is unavailable");
                throw new Exception("Redis is unavailable" + ex);
            }
        }

        public async Task RemoveAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            try
            {
                var db = _redis.GetDatabase();
                await db.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis is unavailable");
                throw new Exception("Redis is unavailable" + ex);
            }
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            try
            {
                var db = _redis.GetDatabase();
                return await db.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis is unavailable");
                throw new Exception("Redis is unavailable" + ex);
            }
        }

        public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            try
            {
                var db = _redis.GetDatabase();
                await db.StringSetAsync(key, value, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis is unavailable");
                throw new Exception("Redis is unavailable" + ex);
            }
        }
    }
}
