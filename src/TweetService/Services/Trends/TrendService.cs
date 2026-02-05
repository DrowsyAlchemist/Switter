using AutoMapper;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TweetService.DTOs;
using TweetService.Interfaces.Data.Repositories;
using TweetService.Interfaces.Infrastructure;
using TweetService.Interfaces.Services;
using TweetService.Models.Options;

namespace TweetService.Services.Trends
{
    public class TrendService : ITrendService
    {
        private readonly TrendCalculator _trendCalculator;
        private readonly ITweetRepository _tweetRepository;
        private readonly IRedisService _redisService;
        private readonly TrendsOptions _options;
        private readonly IMapper _mapper;

        public TrendService(
            TrendCalculator trendCalculator,
            ITweetRepository tweetRepository,
            IRedisService redisService,
            IOptions<TrendsOptions> options,
            IMapper mapper)
        {
            _tweetRepository = tweetRepository;
            _trendCalculator = trendCalculator;
            _redisService = redisService;
            _options = options.Value;
            _mapper = mapper;
        }

        public async Task<List<string>> GetTrendCategoriesAsync(int page, int pageSize)
        {
            List<string>? trendHashtags = null;

            int maxIndex = page * pageSize;
            if (maxIndex <= _options.Cache.TrendHashtagsCacheSize)
                trendHashtags = await GetTrendCategoriesFromCacheAsync();

            if (trendHashtags == null || trendHashtags.Count < maxIndex)
            {
                trendHashtags = await _trendCalculator.CalculateTrendHashtagsByUsageAsync(maxIndex);
                await SaveTrendCategoriesToCacheAsync(trendHashtags);
            }
            return trendHashtags.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }

        public async Task<List<TweetDto>> GetTrendTweetsAsync(int page, int pageSize)
        {
            List<Guid>? trendTweetIds = null;

            int maxIndex = page * pageSize;
            if (maxIndex <= _options.Cache.TrendTweetsCountCacheSize)
                trendTweetIds = await GetTrendsTweetIdsFromCacheAsync();

            if (trendTweetIds == null || trendTweetIds.Count < maxIndex)
            {
                trendTweetIds = await _trendCalculator.CalculateTrendTweetByLastLikesIdsAsync(maxIndex);
                await SaveTrendTweetsToCacheAsync(trendTweetIds);
            }
            var trendTweets = await _tweetRepository.GetByIdsAsync(trendTweetIds, page, pageSize);
            return _mapper.Map<List<TweetDto>>(trendTweets);
        }

        public async Task<List<Guid>> GetTrendTweetIdsAsync(int page, int pageSize)
        {
            List<Guid>? trendTweetIds = null;

            int maxIndex = page * pageSize;
            if (maxIndex <= _options.Cache.TrendTweetsCountCacheSize)
                trendTweetIds = await GetTrendsTweetIdsFromCacheAsync();

            if (trendTweetIds == null || trendTweetIds.Count < maxIndex)
            {
                trendTweetIds = await _trendCalculator.CalculateTrendTweetByLastLikesIdsAsync(maxIndex);
                await SaveTrendTweetsToCacheAsync(trendTweetIds);
            }
            return trendTweetIds.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }

        public async Task<List<Guid>> GetTrendTweetIdsAsync(string hashtag, int page, int pageSize)
        {
            var trendTweetIds = await _trendCalculator.CalculateTrendTweetByLastLikesIdsAsync(int.MaxValue);
            var trendTweets = await _tweetRepository.GetByHashtagAsync(trendTweetIds, hashtag, page, pageSize);
            return _mapper.Map<List<TweetDto>>(trendTweets);
        }

        private async Task<List<string>> GetTrendCategoriesFromCacheAsync()
        {
            var trendsJson = await _redisService.GetAsync(_options.Cache.KeyForTrendHashtags);
            if (trendsJson != null)
            {
                var trendHashtags = JsonSerializer.Deserialize<List<string>>(trendsJson);
                if (trendHashtags == null)
                    throw new JsonException("Cant deserialize trends from redis.");
                return trendHashtags;
            }
            return new List<string>();
        }

        private async Task SaveTrendCategoriesToCacheAsync(List<string> trendCategories)
        {
            var trendCategoriesToSave = trendCategories.Take(_options.Cache.TrendHashtagsCacheSize).ToList();
            var trendsJson = JsonSerializer.Serialize(trendCategoriesToSave);
            await _redisService.SetAsync(_options.Cache.KeyForTrendHashtags, trendsJson, TimeSpan.FromMinutes(_options.Cache.ExpiryInMinutes));
        }

        private async Task<List<Guid>> GetTrendsTweetIdsFromCacheAsync()
        {
            var trendsJson = await _redisService.GetAsync(_options.Cache.KeyForTrendTweets);
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
            var trendTweetsToSave = trendTweets.Take(_options.Cache.TrendHashtagsCacheSize).ToList();
            var trendsJson = JsonSerializer.Serialize(trendTweetsToSave);
            await _redisService.SetAsync(_options.Cache.KeyForTrendTweets, trendsJson, TimeSpan.FromMinutes(_options.Cache.ExpiryInMinutes));
        }
    }
}