namespace UserService.Interfaces.Infrastructure
{
    public interface IRedisService
    {
        Task SetAsync(string key, string value, TimeSpan? expiry = null);
        Task<string?> GetAsync(string key);
        Task RemoveAsync(string key);
        Task<bool> KeyExistsAsync(string key);
    }
}
