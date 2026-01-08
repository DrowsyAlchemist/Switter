using TweetService.Exceptions;
using TweetService.Interfaces.Data;
using TweetService.Interfaces.Services;
using TweetService.Models;

namespace TweetService.Services
{
    public class HashtagService : IHashtagService
    {
        private readonly IHashtagRepository _hashtagRepository;
        private readonly ITweetRepository _tweetRepository;
        private readonly ITweetHashtagRepository _tweetHashtagRepository;

        public HashtagService(IHashtagRepository hashtagRepository,
            ITweetRepository tweetRepository,
            ITweetHashtagRepository tweetHashtagRepository)
        {
            _hashtagRepository = hashtagRepository;
            _tweetRepository = tweetRepository;
            _tweetHashtagRepository = tweetHashtagRepository;
        }

        public async Task ProcessHashtagsAsync(Guid tweetId)
        {
            var tweet = await _tweetRepository.GetByIdAsync(tweetId);
            if (tweet == null)
                throw new TweetNotFoundException(tweetId);

            var content = tweet.Content;
            var hashtags = ExtractHashtags(content);

            var existingHashtags = await _hashtagRepository.GetExists(hashtags);
            var newHashtags = hashtags.Except(existingHashtags).ToList();

            await _hashtagRepository.AddRangeAsync(newHashtags);
            await _hashtagRepository.IncrementUsageCounterAsync(existingHashtags);
        }

        private static List<string> ExtractHashtags(string content)
        {
            List<string> hashtags = content
                .Split(' ')
                .Where(w => w.StartsWith('#'))
                .Select(w => w.Substring(1))
                .ToList();

            return hashtags;
        }
    }
}
