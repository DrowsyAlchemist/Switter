using AutoMapper;
using System.Text.Json;
using TweetService.DTOs;
using TweetService.Interfaces.Data.Repositories;
using TweetService.Interfaces.Infrastructure;
using TweetService.Interfaces.Services;

namespace TweetService.Services.Trends
{
    public class TrendService : ITrendService
    {
        private const string KeyForCachedTrendHashtags = "KeyForTrendHashtags";
        private const string KeyForCachedTrendTweets = "KeyForTrendTweets";
        private const int TrendHashtagsCacheSize = 100;
        private const int TrendTweetsCountCacheSize = 500;
        private const int CacheExpiryInMinutes = 30;

        private readonly TrendCalculator _trendCalculator;
        private readonly ITweetRepository _tweetRepository;
        private readonly IRedisService _redisService;
        private readonly IMapper _mapper;

        public TrendService(
            TrendCalculator trendCalculator,
            ITweetRepository tweetRepository,
            IRedisService redisService,
            IMapper mapper)
        {
            _tweetRepository = tweetRepository;
            _trendCalculator = trendCalculator;
            _redisService = redisService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<string>> GetTrendCategoriesAsync(int page, int pageSize)
        {
            IEnumerable<string>? trendHashtags = null;

            int maxIndex = page * pageSize;
            if (maxIndex <= TrendHashtagsCacheSize)
                trendHashtags = await GetTrendCategoriesFromCacheAsync();

            if (trendHashtags == null || trendHashtags.Count() < maxIndex)
            {
                trendHashtags = await _trendCalculator.CalculateTrendHashtagsByUsageAsync(maxIndex);
                await SaveTrendCategoriesToCacheAsync(trendHashtags);
            }
            return trendHashtags.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }

        public async Task<IEnumerable<TweetDto>> GetTrendTweetsAsync(int page, int pageSize)
        {
            IEnumerable<Guid>? trendTweetIds = null;

            int maxIndex = page * pageSize;
            if (maxIndex <= TrendTweetsCountCacheSize)
                trendTweetIds = await GetTrendsTweetIdsFromCacheAsync();

            if (trendTweetIds == null || trendTweetIds.Count() < maxIndex)
            {
                trendTweetIds = await _trendCalculator.CalculateTrendTweetByLastLikesIdsAsync(maxIndex);
                await SaveTrendTweetsToCacheAsync(trendTweetIds);
            }
            var trendTweets = await _tweetRepository.GetByIdsAsync(trendTweetIds, page, pageSize);
            return _mapper.Map<IEnumerable<TweetDto>>(trendTweets);
        }

        public async Task<IEnumerable<TweetDto>> GetTrendTweetsAsync(string hashtag, int page, int pageSize)
        {
            var trendTweetIds = await _trendCalculator.CalculateTrendTweetByLastLikesIdsAsync(int.MaxValue);
            var trendTweets = await _tweetRepository.GetByHashtagAsync(trendTweetIds, hashtag, page, pageSize);
            return _mapper.Map<IEnumerable<TweetDto>>(trendTweets);
        }

        private async Task<IEnumerable<string>> GetTrendCategoriesFromCacheAsync()
        {
            var trendsJson = await _redisService.GetAsync(KeyForCachedTrendHashtags);
            if (trendsJson != null)
            {
                var trendHashtags = JsonSerializer.Deserialize<List<string>>(trendsJson);
                if (trendHashtags == null)
                    throw new JsonException("Cant deserialize trends from redis.");
                return trendHashtags;
            }
            return new List<string>();
        }

        private async Task SaveTrendCategoriesToCacheAsync(IEnumerable<string> trendCategories)
        {
            var trendCategoriesToSave = trendCategories.Take(TrendHashtagsCacheSize).ToList();
            var trendsJson = JsonSerializer.Serialize(trendCategoriesToSave);
            await _redisService.SetAsync(KeyForCachedTrendHashtags, trendsJson, TimeSpan.FromMinutes(CacheExpiryInMinutes));
        }

        private async Task<List<Guid>> GetTrendsTweetIdsFromCacheAsync()
        {
            var trendsJson = await _redisService.GetAsync(KeyForCachedTrendTweets);
            if (trendsJson != null)
            {
                var trendTweetIdsFromJson = JsonSerializer.Deserialize<List<string>>(trendsJson);
                if (trendTweetIdsFromJson == null)
                    throw new JsonException("Cant deserialize trends from redis.");

                if (trendTweetIdsFromJson.Count != 0)
                    return trendTweetIdsFromJson.Select(s => Guid.Parse(s)).ToList();
            }
            return new List<Guid>();
        }

        private async Task SaveTrendTweetsToCacheAsync(IEnumerable<Guid> trendTweets)
        {
            var trendTweetsToSave = trendTweets.Take(TrendHashtagsCacheSize).ToList();
            var trendsJson = JsonSerializer.Serialize(trendTweetsToSave);
            await _redisService.SetAsync(KeyForCachedTrendTweets, trendsJson, TimeSpan.FromMinutes(CacheExpiryInMinutes));
        }
    }
}