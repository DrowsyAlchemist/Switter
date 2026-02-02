using TweetService.Interfaces.Services;
using TweetService.Services.Trends;

namespace TweetService.Services.Decorators.WithTrendFiller
{
    public class HashtagServiceWithTrendFiller : IHashtagService
    {
        private readonly TrendFiller _trendFiller;
        private readonly IHashtagService _hashtagService;

        public HashtagServiceWithTrendFiller(IHashtagService hashtagService, TrendFiller trendFiller)
        {
            _trendFiller = trendFiller;
            _hashtagService = hashtagService;
        }

        public async Task<IEnumerable<string>> ProcessHashtagsAsync(Guid tweetId, string content)
        {
            var hashtags = await _hashtagService.ProcessHashtagsAsync(tweetId, content);
            if (hashtags.Any() == false)
                return [];

            await _trendFiller.SetHashtagsUsageAsync(hashtags);
            return hashtags;
        }
    }
}
