using FeedService.Interfaces.Data;
using FeedService.Models;
using FeedService.Models.Options;
using FluentAssertions.Common;
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

        private static string GetFeedKey(Guid userId) => $"feed:{userId}";
        private static string GetFeedCounterKey(Guid userId) => $"feed:{userId}:counter";

        public async Task AddToFeedAsync(Guid userId, FeedItem item)
        {
            var feedKey = GetFeedKey(userId);

            // Используем Sorted Set для автоматической сортировки по времени
            var score = GetScore(item.CreatedAt);
            var value = SerializeFeedItem(item);

            await _redis.SortedSetAddAsync(feedKey, value, score);

            // Обрезаем ленту если нужно
            await TrimFeedAsync(userId, _options.MaxFeedSize);

            // Инкрементируем счетчик
            await IncrementFeedCounterAsync(userId);

            _logger.LogDebug("Added tweet {TweetId} to feed of user {UserId}",
                item.TweetId, userId);
        }

        public Task AddToFeedAsync(Guid userId, IEnumerable<FeedItem> items)
        {
            throw new NotImplementedException();
        }

        public async Task<List<FeedItem>> GetFeedPageAsync(Guid userId, int start, int count)
        {
            var feedKey = GetFeedKey(userId);

            // Получаем страницу из Sorted Set (сортировка по убыванию score)
            var values = await _redis.SortedSetRangeByRankAsync(
                feedKey,
                start,
                start + count - 1,
                Order.Descending);

            var items = new List<FeedItem>();

            foreach (var value in values)
            {
                if (!value.IsNullOrEmpty)
                {
                    var item = DeserializeFeedItem(value!);
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }
            }

            return items;
        }

        public async Task RemoveFromFeedAsync(Guid userId, Guid tweetId)
        {
            var feedKey = GetFeedKey(userId);

            // Чтобы удалить элемент, нам нужно его значение
            // Для этого можно хранить дополнительный индекс
            var itemKey = $"feed:{userId}:index:{tweetId}";
            var itemValue = await _redis.StringGetAsync(itemKey);

            if (!itemValue.IsNullOrEmpty)
            {
                await _redis.SortedSetRemoveAsync(feedKey, itemValue);
                await _redis.KeyDeleteAsync(itemKey);

                _logger.LogDebug("Removed tweet {TweetId} from feed of user {UserId}",
                    tweetId, userId);
            }
        }

        public async Task TrimFeedAsync(Guid userId, long maxLength)
        {
            var feedKey = GetFeedKey(userId);

            // Удаляем старые элементы, оставляя только maxLength самых новых
            await _redis.SortedSetRemoveRangeByRankAsync(feedKey, 0, -maxLength - 1);

            _logger.LogDebug("Trimmed feed for user {UserId} to {MaxLength} items",
                userId, maxLength);
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
            await _redis.KeyDeleteAsync(GetFeedCounterKey(userId));

            _logger.LogInformation("Cleared feed for user {UserId}", userId);
        }

        public async Task<bool> FeedExistsAsync(Guid userId)
        {
            var feedKey = GetFeedKey(userId);
            return await _redis.KeyExistsAsync(feedKey);
        }

        public async Task IncrementFeedCounterAsync(Guid userId)
        {
            var counterKey = GetFeedCounterKey(userId);
            await _redis.StringIncrementAsync(counterKey);
        }

        public async Task<long> GetFeedCounterAsync(Guid userId)
        {
            var counterKey = GetFeedCounterKey(userId);
            var value = await _redis.StringGetAsync(counterKey);
            return value.HasValue ? (long)value : 0;
        }

        // Вспомогательные методы
        private static double GetScore(DateTime createdAt)
        {
            // Используем Unix timestamp как score для сортировки по времени
            // Умножаем на -1 для сортировки от новых к старым
            return -createdAt.ToDateTimeOffset().ToUnixTimeSeconds();
        }

        private static string SerializeFeedItem(FeedItem item)
        {
            return JsonSerializer.Serialize(new
            {
                item.TweetId,
                item.CreatedAt,
                item.Score
            });
        }

        private static FeedItem? DeserializeFeedItem(string json)
        {
            try
            {
                var data = JsonSerializer.Deserialize<FeedItemData>(json);
                if (data == null) return null;

                return new FeedItem
                {
                    TweetId = data.TweetId,
                    CreatedAt = data.CreatedAt,
                    Score = data.Score
                };
            }
            catch
            {
                return null;
            }
        }

        private class FeedItemData
        {
            public Guid TweetId { get; set; }
            public Guid AuthorId { get; set; }
            public DateTime CreatedAt { get; set; }
            public double Score { get; set; }
        }
    }
}
