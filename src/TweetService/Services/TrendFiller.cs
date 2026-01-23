using TweetService.Interfaces.Infrastructure;

namespace TweetService.Services
{
    public class TrendFiller
    {
        private const string KeyForLastUsedHashtags = "KeyForLastUsedHashtags";
        private const string KeyForLastLikedTweets = "KeyForLastLikedTweets";

        private readonly IRedisService _redisService;

        public TrendFiller(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task SetHashtagsUsageAsync(IEnumerable<string> hashtags)
        {
            ArgumentNullException.ThrowIfNull(hashtags);

            foreach (var hashtag in hashtags)
                if (string.IsNullOrEmpty(hashtag))
                    throw new ArgumentException("Hashtags contains null or empty elements.");

            await _redisService.AddToListAsync(KeyForLastUsedHashtags, hashtags);
        }

        public async Task SetLikedTweetAsync(Guid likedTweetId)
        {
            await _redisService.AddToListAsync(KeyForLastLikedTweets, [likedTweetId.ToString()]);
        }
    }
}
