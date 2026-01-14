using TweetService.DTOs;
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

        public async Task ProcessHashtagsAsync(Guid tweetId, string content)
        {
            ArgumentNullException.ThrowIfNull(content);
            var hashtags = ExtractHashtags(content);
            if (hashtags.Count == 0)
                return;

            foreach (var hashtag in hashtags)
            {
                if (hashtag.Length == 0 || hashtag.Length > 50)
                    throw new InvalidHashtagException($"Invalid hashtag length ({hashtag.Length}).");
            }

            var existingHashtags = await _hashtagRepository.GetExists(hashtags);
            var newHashtags = hashtags.Except(existingHashtags).ToList();

            await _hashtagRepository.AddRangeAsync(newHashtags);
            await _hashtagRepository.IncrementUsageCounterAsync(existingHashtags);

            var hashtagIds = await _hashtagRepository.GetIdByTag(hashtags);
            var tweetHashtags = new List<TweetHashtag>();
            foreach (var id in hashtagIds)
                tweetHashtags.Add(new TweetHashtag { HashtagId = id, TweetId = tweetId });
            await _tweetHashtagRepository.AddRangeAsync(tweetHashtags);
        }

        private List<string> ExtractHashtags(string content)
        {
            return content
                .Split(' ')
                .Where(w => w.StartsWith('#'))
                .Select(w => w.Substring(1))
                .Distinct()
                .ToList();
        }
    }
}
