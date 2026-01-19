using AutoMapper;
using System.Text.Json;
using TweetService.DTOs;
using TweetService.Interfaces.Data;
using TweetService.Interfaces.Infrastructure;
using TweetService.Interfaces.Services;
using TweetService.Models;

namespace TweetService.Services
{
    public class TrendService : ITrendService
    {
        private const string KeyForTrendHashtags = "KeyForTrendHashtags";
        private const string KeyForHashtagUsage = "KeyForHashtagUsages";
        private const string KeyForLikes = "KeyForTweetLikes";
        private const string KeyForTrendTweets = "KeyForTrendTweets";
        private const int TrendHashtagsCacheSize = 100;
        private const int TrendTweetsCountCacheSize = 500;
        private const int TrendsPeriodInHours = 24;
        private const int CacheExpiryInMinutes = 30;

        private readonly ITweetRepository _tweetRepository;
        private readonly IUserTweetRelationship _userTweetRelationship;
        private readonly IRedisService _redisService;
        private readonly IMapper _mapper;

        public TrendService(ITweetRepository tweetRepository,
            IRedisService redisService,
            IMapper mapper,
            IUserTweetRelationship userTweetRelationship)
        {
            _tweetRepository = tweetRepository;
            _redisService = redisService;
            _mapper = mapper;
            _userTweetRelationship = userTweetRelationship;
        }

        public async Task<List<string>> GetTrendCategoriesAsync(int page, int pageSize)
        {
            string? trendsJson;
            List<string>? trendHashtags;

            int maxIndex = page * pageSize;
            if (maxIndex <= TrendHashtagsCacheSize)
            {
                trendHashtags = await GetTrendCategoriesFromCacheAsync();
                if (trendHashtags.Count >= maxIndex)
                    return trendHashtags.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }

            trendHashtags = await CalculateTrendCategoriesAsync();

            trendsJson = JsonSerializer.Serialize(trendHashtags.Take(TrendHashtagsCacheSize).ToList());
            await _redisService.SetAsync(KeyForTrendHashtags, trendsJson, TimeSpan.FromMinutes(CacheExpiryInMinutes));

            return trendHashtags.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }

        public async Task<List<TweetDto>> GetTrendTweetsAsync(Guid? userId, int page, int pageSize)
        {
            int maxIndex = page * pageSize;
            List<Guid>? trendTweetIds = null;

            if (maxIndex <= TrendTweetsCountCacheSize)
                trendTweetIds = await GetTrendsTweetIdsFromCacheAsync();

            if (trendTweetIds!.Count < maxIndex)
            {
                trendTweetIds = await CalculateTrendsTweetIdsAsync(maxIndex);
                var trendsJson = JsonSerializer.Serialize(trendTweetIds.Take(TrendTweetsCountCacheSize).ToList());
                await _redisService.SetAsync(KeyForTrendTweets, trendsJson, TimeSpan.FromMinutes(CacheExpiryInMinutes));
            }
            var trendTweets = await _tweetRepository.GetByIdsAsync(trendTweetIds, page, pageSize);
            var tweetDtos = await GetTweetDtosWithUserRelationshipsAsync(trendTweets, userId);
            return tweetDtos;
        }

        public async Task<List<TweetDto>> GetTrendTweetsAsync(string hashtag, Guid? userId, int page, int pageSize)
        {
            int maxIndex = page * pageSize;
            var trendTweetIds = await CalculateTrendsTweetIdsAsync(maxIndex);
            var trendTweets = await _tweetRepository.GetByHashtagAsync(trendTweetIds, hashtag, page, pageSize);
            var tweetDtos = await GetTweetDtosWithUserRelationshipsAsync(trendTweets, userId);
            return tweetDtos;
        }

        private async Task<List<string>> GetTrendCategoriesFromCacheAsync()
        {
            var trendsJson = await _redisService.GetAsync(KeyForTrendHashtags);
            if (trendsJson != null)
            {
                var trendHashtags = JsonSerializer.Deserialize<List<string>>(trendsJson);
                if (trendHashtags == null)
                    throw new JsonException("Cant deserialize trends from redis.");
                return trendHashtags;
            }
            return new List<string>();
        }

        private async Task<List<Guid>> GetTrendsTweetIdsFromCacheAsync()
        {
            var trendsJson = await _redisService.GetAsync(KeyForTrendTweets);
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

        private async Task<List<string>> CalculateTrendCategoriesAsync()
        {
            var period = TimeSpan.FromHours(TrendsPeriodInHours);
            var lastHashtags = await _redisService.GetListFromDateAsync(KeyForHashtagUsage, period);
            if (lastHashtags.Count == 0)
                return lastHashtags;

            var hashtagsUsage = new Dictionary<string, int>();
            foreach (var tag in lastHashtags)
            {
                if (hashtagsUsage.ContainsKey(tag))
                    hashtagsUsage[tag]++;
                else
                    hashtagsUsage.Add(tag, 1);
            }
            return hashtagsUsage
                .OrderByDescending(h => h.Value)
                .Select(h => h.Key)
                .ToList();
        }

        private async Task<List<Guid>> CalculateTrendsTweetIdsAsync(int count)
        {
            var period = TimeSpan.FromHours(TrendsPeriodInHours);
            var lastLikedIds = await _redisService.GetListFromDateAsync(KeyForLikes, period);

            if (lastLikedIds.Count == 0)
                return new List<Guid>();

            var likesCount = new Dictionary<string, int>();
            foreach (string tweetId in lastLikedIds)
            {
                if (likesCount.ContainsKey(tweetId))
                    likesCount[tweetId]++;
                else
                    likesCount.Add(tweetId, 1);
            }
            return likesCount
                .OrderByDescending(t => t.Value)
                .Take(count)
                .Select(t => Guid.Parse(t.Key))
                .ToList();
        }

        private async Task<List<TweetDto>> GetTweetDtosWithUserRelationshipsAsync(List<Tweet> tweets, Guid? userId)
        {
            var tweetDtos = _mapper.Map<List<TweetDto>>(tweets);
            if (userId.HasValue)
                tweetDtos = await _userTweetRelationship.GetTweetsWithRelationshipsAsync(tweetDtos, userId.Value);
            return tweetDtos;
        }
    }
}