using Bogus;
using FluentAssertions.Common;
using StackExchange.Redis;
using TweetService.Interfaces.Infrastructure;

namespace TweetService.Services.Infrastructure
{
    public class RedisService : IRedisService
    {
        private const string DefaultFieldNameForStream = "EntityId";
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger _logger;

        public RedisService(IConnectionMultiplexer redis, ILogger<RedisService> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        public async Task AddToListAsync(string key, IEnumerable<string> values)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Any() == false)
                return;

            try
            {
                var db = _redis.GetDatabase();
                var entries = new List<NameValueEntry>();

                foreach (var value in values)
                    entries.Add(new NameValueEntry(DefaultFieldNameForStream, value));

                await db.StreamAddAsync(key, entries.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis is unavailable");
                throw new Exception("Redis is unavailable" + ex.Message, ex);
            }
        }

        public async Task<List<string>> GetListFromDateAsync(string key, TimeSpan period)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            try
            {
                var startDateTime = DateTime.UtcNow - period;
                long startTimeStamp = startDateTime.ToDateTimeOffset().ToUnixTimeMilliseconds();
                var db = _redis.GetDatabase();
                string startId = $"{startTimeStamp}-0";

                var entries = await db.StreamRangeAsync(
                    key,
                    minId: startId,
                    maxId: "+",
                    count: int.MaxValue,
                    Order.Ascending
                );
                var result = new List<string>();

                foreach (var entry in entries)
                    if (entry.Values.Any(v => v.Name == DefaultFieldNameForStream))
                        result.Add(entry[DefaultFieldNameForStream].ToString());

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis is unavailable");
                throw new Exception("Redis is unavailable" + ex.Message, ex);
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
                throw new Exception("Redis is unavailable" + ex.Message, ex);
            }
        }

        public async Task RemoveKeyAsync(string key)
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
                throw new Exception("Redis is unavailable" + ex.Message, ex);
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
                throw new Exception("Redis is unavailable" + ex.Message, ex);
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
                throw new Exception("Redis is unavailable" + ex.Message, ex);
            }
        }
    }
}
