#if DEBUG
using AuthService.Interfaces.Infrastructure;

namespace AuthService.Tests.MockServices
{
    public class MockRedis : IRedisService
    {
        private readonly Dictionary<string, string?> _storage = new Dictionary<string, string?>();

        public Task<string?> GetAsync(string key)
        {
            return Task.FromResult(_storage[key]);
        }

        public Task<bool> KeyExistsAsync(string key)
        {
            return Task.FromResult(_storage.ContainsKey(key));
        }

        public Task RemoveAsync(string key)
        {
            _storage.Remove(key);
            return Task.CompletedTask;
        }

        public Task SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            _storage[key] = value;
            return Task.CompletedTask;
        }
    }
}
#endif