using Microsoft.Extensions.Options;
using TweetService.Interfaces.Infrastructure;
using TweetService.Models.Options;

namespace TweetService.Services.Trends
{
    public class TrendCalculator
    {
        private readonly IRedisService _redisService;
        private readonly TrendsOptions _options;

        public TrendCalculator(IRedisService redisService, IOptions<TrendsOptions> options)
        {
            _redisService = redisService;
            _options = options.Value;
        }

        public async Task<List<string>> CalculateTrendHashtagsByUsageAsync(int count)
        {
            if (count < 0)
                throw new ArgumentException("Count should be positive.");
            if (count == 0)
                return [];

            var period = TimeSpan.FromHours(_options.TrendsPeriodInHours);
            var lastHashtags = await _redisService.GetListFromDateAsync(_options.KeyForLastUsedHashtags, period);
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
                .Take(count)
                .OrderByDescending(h => h.Value)
                .Select(h => h.Key)
                .ToList();
        }

        public async Task<List<Guid>> CalculateTrendTweetByLastLikesIdsAsync(int count)
        {
            if (count < 0)
                throw new ArgumentException("Count should be positive.");
            if (count == 0)
                return [];

            var period = TimeSpan.FromHours(_options.TrendsPeriodInHours);
            var lastLikedIds = await _redisService.GetListFromDateAsync(_options.KeyForLastLikedTweets, period);

            if (lastLikedIds.Count == 0)
                return [];

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
                .OrderByDescending(h => h.Value)
                .Select(t => Guid.Parse(t.Key))
                .ToList();
        }
    }
}
