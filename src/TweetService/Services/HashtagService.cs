using TweetService.Exceptions;
using TweetService.Interfaces.Data.Repositories;
using TweetService.Interfaces.Services;
using TweetService.Models;

namespace TweetService.Services
{
    public class HashtagService : IHashtagService
    {
        private const int MaxHashtagLength = 50;
        private readonly IHashtagRepository _hashtagRepository;
        private readonly ITweetHashtagRepository _tweetHashtagRepository;

        public HashtagService(
            IHashtagRepository hashtagRepository,
            ITweetHashtagRepository tweetHashtagRepository)
        {
            _hashtagRepository = hashtagRepository;
            _tweetHashtagRepository = tweetHashtagRepository;
        }

        public async Task<IEnumerable<string>> ProcessHashtagsAsync(Guid tweetId, string content)
        {
            ArgumentNullException.ThrowIfNull(content);
            var hashtags = ExtractHashtags(content);
            if (hashtags.Any() == false)
                return [];

            var hashtagSet = new HashSet<string>(hashtags);
            foreach (var hashtag in hashtagSet)
            {
                if (hashtag.Length == 0 || hashtag.Length > MaxHashtagLength)
                    throw new InvalidHashtagException($"Invalid hashtag length ({hashtag.Length}).");
            }

            var existingHashtags = await _hashtagRepository.GetExists(hashtagSet);
            var newHashtags = hashtagSet.Except(existingHashtags).ToList();

            await _hashtagRepository.AddRangeAsync(newHashtags);
            await _hashtagRepository.IncrementUsageCounterAsync(existingHashtags);

            var hashtagIds = await _hashtagRepository.GetIdByTag(hashtags);
            var tweetHashtags = new List<TweetHashtag>();
            foreach (var id in hashtagIds)
                tweetHashtags.Add(new TweetHashtag { HashtagId = id, TweetId = tweetId });
            await _tweetHashtagRepository.AddRangeAsync(tweetHashtags);
            return hashtags;
        }

        private static IEnumerable<string> ExtractHashtags(string content)
        {
            return content
                .Split(' ')
                .Where(w => w.StartsWith('#'))
                .Select(w => w.Substring(1))
                .Distinct();
        }
    }
}
