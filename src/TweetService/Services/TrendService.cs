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
        private const string KeyForTrendHashtags = "TrendHashtags";
        private const string KeyForHashtagUsage = "HashtagUsages";
        private const string KeyForLikes = "TweetLikes";
        private const string KeyForTrendTweets = "TrendTweets";
        private const int TrendHashtagsCount = 100;
        private const int TrendTweetsCount = 500;

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

        public async Task<List<string>> GetTrendCategoriesAsync()
        {
            var trendsJson = await _redisService.GetAsync(KeyForTrendHashtags);
            List<string>? trendHashtags = new();
            if (trendsJson != null)
            {
                trendHashtags = JsonSerializer.Deserialize<List<string>>(trendsJson);
                if (trendHashtags == null)
                    throw new JsonException("Cant deserialize trends from redis.");
                return trendHashtags;
            }

            var startDateTime = DateTime.UtcNow - TimeSpan.FromHours(24);
            var lastHashtags = await _redisService.GetListFromDateAsync(KeyForHashtagUsage, startDateTime);
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
            trendHashtags = hashtagsUsage
                .OrderByDescending(h => h.Value)
                .Take(TrendHashtagsCount)
                .Select(h => h.Key)
                .ToList();

            trendsJson = JsonSerializer.Serialize(trendHashtags);
            await _redisService.SetAsync(KeyForTrendHashtags, trendsJson, TimeSpan.FromMinutes(30));
            return trendHashtags;
        }

        public async Task<List<TweetDto>> GetTrendTweetsAsync(Guid? userId)
        {
            var trendTweetIds = await GetTrendsTweetIdsFromCacheAsync();

            if (trendTweetIds == null)
            {
                trendTweetIds = await GetTrendsTweetIdsAsync();
                var trendsJson = JsonSerializer.Serialize(trendTweetIds);
                await _redisService.SetAsync(KeyForTrendTweets, trendsJson, TimeSpan.FromMinutes(30));
            }
            if (trendTweetIds.Count == 0)
                return new List<TweetDto>();

            var trendTweets = await _tweetRepository.GetByIdsAsync(trendTweetIds);
            var tweetDtos = await GetTweetDtosWithUserRelationshipsAsync(trendTweets, userId);
            return tweetDtos;
        }

        public async Task<List<TweetDto>> GetTrendTweetsAsync(string hashtag, Guid? userId)
        {

            var trendTweetIds = await GetTrendsTweetIdsAsync();
            if (trendTweetIds.Count == 0)
                return new List<TweetDto>();

            var trendTweets = await _tweetRepository.GetByHashtagAsync(trendTweetIds, hashtag);
            var tweetDtos = await GetTweetDtosWithUserRelationshipsAsync(trendTweets, userId);
            return tweetDtos;
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

        private async Task<List<Guid>> GetTrendsTweetIdsAsync()
        {
            var startDateTime = DateTime.UtcNow - TimeSpan.FromHours(24);
            var lastLikesJson = await _redisService.GetListFromDateAsync(KeyForLikes, startDateTime);

            if (lastLikesJson.Count == 0)
                return new List<Guid>();

            var likesCount = new Dictionary<string, int>();
            foreach (string likeJson in lastLikesJson)
            {
                var like = JsonSerializer.Deserialize<LikeUsage>(likeJson);
                if (like == null)
                    throw new JsonException("Cant deserialize like from redis.");

                string tweetId = like.TweetId.ToString();
                if (likesCount.ContainsKey(tweetId))
                    likesCount[tweetId]++;
                else
                    likesCount.Add(tweetId, 1);
            }
            return likesCount
                .OrderByDescending(t => t.Value)
                .Take(TrendTweetsCount)
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