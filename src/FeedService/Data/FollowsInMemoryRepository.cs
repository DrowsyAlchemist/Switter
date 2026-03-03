using FeedService.Interfaces.Data;
using FeedService.Interfaces.Infrastructure;

namespace FeedService.Data
{
    public class FollowsInMemoryRepository : IFollowsRepository
    {
        private const int FollowersCacheTtlInMinutes = 10;
        private readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(FollowersCacheTtlInMinutes);
        private readonly Dictionary<Guid, HashSet<Guid>> _followersCache = new();

        private readonly IProfileServiceClient _profileServiceClient;
        private readonly ILogger<FollowsInMemoryRepository> _logger;

        public FollowsInMemoryRepository(IProfileServiceClient profileServiceClient, ILogger<FollowsInMemoryRepository> logger)
        {
            _profileServiceClient = profileServiceClient;
            _logger = logger;
        }

        public async Task<IEnumerable<Guid>> GetFollowersAsync(Guid followingId)
        {
            if (_followersCache.TryGetValue(followingId, out var cachedFollowers))
                return cachedFollowers;

            try
            {
                var followers = await _profileServiceClient.GetFollowersAsync(followingId);

                if (_followersCache.ContainsKey(followingId))
                    _followersCache[followingId] = followers.ToHashSet();
                else
                    _followersCache.Add(followingId, followers.ToHashSet());

                _ = Task.Delay(_cacheTtl).ContinueWith(_ =>
                    _followersCache.Remove(followingId));

                return followers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get followers for user {UserId}", followingId);
                return new List<Guid>();
            }
        }

        public async Task<IEnumerable<Guid>> GetFollowingsAsync(Guid followerId, int count)
        {
            try
            {
                return await _profileServiceClient.GetFollowingsAsync(followerId, count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get followings for user {UserId}", followerId);
                return new List<Guid>();
            }
        }

        public Task AddFollowerAsync(Guid followerId, Guid followingId)
        {
            if (_followersCache.TryGetValue(followingId, out HashSet<Guid>? value))
                value.Add(followerId);
            else
                _followersCache.Add(followingId, [followerId]);

            return Task.CompletedTask;
        }

        public Task RemoveFollowerAsync(Guid followerId, Guid followingId)
        {
            if (_followersCache.TryGetValue(followingId, out HashSet<Guid>? value))
                value.Remove(followerId);

            return Task.CompletedTask;
        }
    }
}