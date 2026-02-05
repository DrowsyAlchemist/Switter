using FeedService.Interfaces.Data;
using FeedService.Models;
using FeedService.Models.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace FeedService.Data
{
    public class RedisFeedRepository : IFeedRepository
    {
        private readonly IDatabase _redis;
        private readonly FeedOptions _options;
        private readonly ILogger<RedisFeedRepository> _logger;

        public RedisFeedRepository(IConnectionMultiplexer redis, IOptions<FeedOptions> options, ILogger<RedisFeedRepository> logger)
        {
            _redis = redis.GetDatabase();
            _options = options.Value;
            _logger = logger;
        }

        public async Task AddToFeedAsync(Guid userId, FeedItem item)
        {
            ArgumentNullException.ThrowIfNull(item);
            var feedKey = GetFeedKey(userId);
            var feedJson = SerializeFeedItem(item);

            await _redis.SortedSetAddAsync(feedKey, feedJson, item.Score);
            await TrimFeedAsync(userId, _options.MaxFeedSize);

            _logger.LogDebug("Added tweet {TweetId} to feed of user {UserId}",
                item.TweetId, userId);
        }

        public async Task AddToFeedAsync(Guid userId, List<FeedItem> items)
        {
            ArgumentNullException.ThrowIfNull(items);
            if (items.Count == 0)
                return;

            var feedKey = GetFeedKey(userId);

            var batch = _redis.CreateBatch();

            var sortedSetEntries = items
                .Select(item => new SortedSetEntry(SerializeFeedItem(item), item.Score))
                .ToArray();

            var addTask = batch.SortedSetAddAsync(feedKey, sortedSetEntries);
            batch.Execute();
            await addTask;

            await TrimFeedAsync(userId, _options.MaxFeedSize);

            _logger.LogDebug("Added {Count} tweets to feed of user {UserId}",
                items.Count, userId);
        }

        public async Task<List<FeedItem>> GetFeedPageAsync(Guid userId, int start, int count)
        {
            var feedKey = GetFeedKey(userId);

            var values = await _redis.SortedSetRangeByRankAsync(
                feedKey,
                start,
                start + count - 1,
                Order.Descending);

            var feedItems = new List<FeedItem>();

            foreach (var value in values)
            {
                if (value.IsNullOrEmpty == false)
                {
                    var feedItem = DeserializeFeedItem(value!);
                    if (feedItem != null)
                        feedItems.Add(feedItem);
                }
            }
            return feedItems;
        }

        public async Task<long> GetFeedLengthAsync(Guid userId)
        {
            var feedKey = GetFeedKey(userId);
            return await _redis.SortedSetLengthAsync(feedKey);
        }

        public async Task ClearFeedAsync(Guid userId)
        {
            var feedKey = GetFeedKey(userId);
            await _redis.KeyDeleteAsync(feedKey);
            _logger.LogInformation("Cleared feed for user {UserId}", userId);
        }

        public async Task RemoveFromFeedAsync(Guid userId, Guid tweetId)
        {
            var feed = await GetFeedPageAsync(userId, 0, _options.MaxFeedSize);

            feed.RemoveAll(i => i.TweetId == tweetId);
            await ClearFeedAsync(userId);
            await AddToFeedAsync(userId, feed);

            _logger.LogInformation("Tweet {tweet} removed from feed. User: {UserId}", tweetId, userId);
        }

        public async Task RemoveUserTweetsFromFeedAsync(Guid feedOwnerId, Guid userToRemoveId)
        {
            var feed = await GetFeedPageAsync(feedOwnerId, 0, _options.MaxFeedSize);

            var removedCount = feed.RemoveAll(i => i.AuthorId == userToRemoveId);
            await ClearFeedAsync(feedOwnerId);
            await AddToFeedAsync(feedOwnerId, feed);

            _logger.LogInformation("{Count} tweets from user {userToRemove} removed from feed. User: {owner}",
                removedCount, userToRemoveId, feedOwnerId);
        }

        private static string GetFeedKey(Guid userId) => $"feed:{userId}";

        private static string SerializeFeedItem(FeedItem item) => JsonSerializer.Serialize(item);

        private FeedItem? DeserializeFeedItem(string json)
        {
            try
            {
                var feedItem = JsonSerializer.Deserialize<FeedItem>(json);
                if (feedItem == null)
                    return null;

                return feedItem;
            }
            catch
            {
                _logger.LogError("Json Deserialize Error. json: {json}", json);
                return null;
            }
        }

        private async Task TrimFeedAsync(Guid userId, long maxLength)
        {
            var feedKey = GetFeedKey(userId);
            await _redis.SortedSetRemoveRangeByRankAsync(feedKey, 0, -maxLength - 1);
            _logger.LogDebug("Trimmed feed for user {UserId} to {MaxLength} items", userId, maxLength);
        }
    }
}