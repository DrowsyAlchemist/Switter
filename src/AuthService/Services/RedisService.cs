using AuthService.Interfaces;
using StackExchange.Redis;

namespace AuthService.Services
{
    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer _redis;

        public RedisService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<string?> GetAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            var db = _redis.GetDatabase();
            return await db.StringGetAsync(key);
        }

        public async Task RemoveAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            var db = _redis.GetDatabase();
            return await db.KeyExistsAsync(key);
        }

        public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var db = _redis.GetDatabase();
            await db.StringSetAsync(key, value, expiry);
        }
    }
}
