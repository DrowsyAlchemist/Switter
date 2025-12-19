namespace TweetService.Interfaces.Infrastructure
{
    public interface IRedisService
    {
        Task<string?> GetAsync(string key);
        Task<List<string>> GetListFromDateAsync(string key, DateTime startDateTime);
        Task<bool> KeyExistsAsync(string key);

        Task SetAsync(string key, string value, TimeSpan? expiry = null);
        Task AddToListAsync(string key, string value);
        Task RemoveAsync(string key);
    }
}
