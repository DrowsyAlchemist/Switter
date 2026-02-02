namespace FeedService.Interfaces.Infrastructure
{
    public interface IRedisService
    {
        Task SetAsync(string key, string value, TimeSpan? expiry = null);
        Task<string?> GetAsync(string key);
        Task<bool> RemoveAsync(string key);
        Task<bool> KeyExistsAsync(string key);
        Task<bool> SetAddAsync(string key, string value);
        Task<bool> SetRemoveAsync(string key, string value);
        Task<long> SetLengthAsync(string key);
    }
}
