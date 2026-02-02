namespace TweetService.Models.Options
{
    public class TrendsOptions
    {
        public required string KeyForLastUsedHashtags { get; set; }
        public required string KeyForLastLikedTweets { get; set; }
        public required int TrendsPeriodInHours { get; set; }
        public required CacheOptions Cache { get; set; }
    }

    public class CacheOptions
    {
        public required string KeyForTrendHashtags { get; set; }
        public required string KeyForTrendTweets { get; set; }
        public required int TrendHashtagsCacheSize { get; set; }
        public required int TrendTweetsCountCacheSize { get; set; }
        public required int ExpiryInMinutes { get; set; }
    }
}
