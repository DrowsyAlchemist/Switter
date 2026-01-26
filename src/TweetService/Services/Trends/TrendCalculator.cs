using TweetService.Interfaces.Infrastructure;

namespace TweetService.Services.Trends
{
    public class TrendCalculator
    {
        private const string KeyForLastUsedHashtags = "KeyForLastUsedHashtags";
        private const string KeyForLastLikedTweets = "KeyForLastLikedTweets";
        private const int TrendsPeriodInHours = 24;

        private readonly IRedisService _redisService;

        public TrendCalculator(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task<IEnumerable<string>> CalculateTrendHashtagsByUsageAsync(int count)
        {
            if (count < 0)
                throw new ArgumentException("Count should be positive.");
            if (count == 0)
                return [];

            var period = TimeSpan.FromHours(TrendsPeriodInHours);
            var lastHashtags = await _redisService.GetListFromDateAsync(KeyForLastUsedHashtags, period);
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
                .Select(h => h.Key)
                .ToList();
        }

        public async Task<IEnumerable<Guid>> CalculateTrendTweetByLastLikesIdsAsync(int count)
        {
            if (count < 0)
                throw new ArgumentException("Count should be positive.");
            if (count == 0)
                return [];

            var period = TimeSpan.FromHours(TrendsPeriodInHours);
            var lastLikedIds = await _redisService.GetListFromDateAsync(KeyForLastLikedTweets, period);

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
                .Select(t => Guid.Parse(t.Key))
                .ToList();
        }
    }
}
