using Microsoft.Extensions.Options;
using TweetService.Interfaces.Infrastructure;
using TweetService.Models.Options;

namespace TweetService.Services.Trends
{
    public class TrendFiller
    {
        private readonly IRedisService _redisService;
        private readonly TrendsOptions _options;

        public TrendFiller(IRedisService redisService, IOptions<TrendsOptions> options)
        {
            _redisService = redisService;
            _options = options.Value;
        }

        public async Task SetHashtagsUsageAsync(IEnumerable<string> hashtags)
        {
            ArgumentNullException.ThrowIfNull(hashtags);

            foreach (var hashtag in hashtags)
                if (string.IsNullOrEmpty(hashtag))
                    throw new ArgumentException("Hashtags contains null or empty elements.");

            await _redisService.AddToListAsync(_options.KeyForLastUsedHashtags, hashtags);
        }

        public async Task SetLikedTweetAsync(Guid likedTweetId)
        {
            await _redisService.AddToListAsync(_options.KeyForLastLikedTweets, [likedTweetId.ToString()]);
        }
    }
}
