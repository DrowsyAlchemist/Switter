using Microsoft.EntityFrameworkCore;
using TweetService.Interfaces.Data;
using TweetService.Models;

namespace TweetService.Data
{
    public class TweetRepository : ITweetRepository
    {
        private readonly TweetDbContext _context;

        public TweetRepository(TweetDbContext tweetDbContext, ILogger<TweetRepository> logger)
        {
            _context = tweetDbContext;
        }

        public async Task<Tweet?> GetByIdAsync(Guid id)
        {
            return await _context.Tweets
                   .AsNoTracking()
                   .Where(t =>
                        t.Id.Equals(id)
                        && t.IsDeleted == false)
                   .FirstOrDefaultAsync();
        }

        public async Task<List<Tweet>> GetByIdsAsync(IEnumerable<Guid> ids, int page, int pageSize)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));
            if (ids.Any() == false)
                return new List<Tweet>();

            return await _context.Tweets
                   .AsNoTracking()
                   .Where(t =>
                        t.IsDeleted == false
                        && ids.Contains(t.Id))
                   .Skip((page - 1) * pageSize)
                   .Take(pageSize)
                   .ToListAsync();
        }

        public async Task<List<Tweet>> GetByHashtagAsync(IEnumerable<Guid> ids, string hashtag, int page, int pageSize)
        {
            if (string.IsNullOrEmpty(hashtag))
                throw new ArgumentException(nameof(hashtag));
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));
            if (ids.Any() == false)
                return new List<Tweet>();

            return await _context.Tweets
                   .Include(t => t.TweetHashtags)
                   .AsNoTracking()
                   .Where(t =>
                        ids.Contains(t.Id)
                        && t.IsDeleted == false
                        && t.TweetHashtags.Any(th => th.Hashtag.Tag.Equals(hashtag)))
                   .Skip((page - 1) * pageSize)
                   .Take(pageSize)
                   .ToListAsync();
        }

        public async Task<List<Tweet>> GetByUserAsync(Guid userId, int page, int pageSize)
        {
            return await _context.Tweets
                   .AsNoTracking()
                   .Where(t =>
                        t.AuthorId.Equals(userId)
                        && t.IsDeleted == false)
                   .Skip((page - 1) * pageSize)
                   .Take(pageSize)
                   .ToListAsync();
        }

        public async Task<List<Guid>> GetIdsByUserAsync(Guid userId, int page, int pageSize)
        {
            return await _context.Tweets
                   .AsNoTracking()
                   .Where(t =>
                        t.AuthorId.Equals(userId)
                        && t.IsDeleted == false)
                   .Select(t => t.Id)
                   .Skip((page - 1) * pageSize)
                   .Take(pageSize)
                   .ToListAsync();
        }

        public async Task<bool> IsRetweetedAsync(Guid tweetId, Guid userId)
        {
            return await _context.Tweets.AnyAsync(
                t => t.AuthorId == userId
                && t.Type == TweetType.Retweet
                && t.ParentTweet!.Id == tweetId);
        }

        public async Task<List<Guid>> GetRetweetedIdsAsync(List<Guid> tweetIds, Guid userId)
        {
            return await _context.Tweets.Where(t =>
                t.AuthorId == userId
                && t.Type == TweetType.Retweet
                && tweetIds.Contains(t.ParentTweet!.Id))
                .Select(t => t.Id)
                .ToListAsync();
        }

        public async Task<List<Tweet>> GetRepliesAsync(Guid tweetId, int page, int pageSize)
        {
            return await _context.Tweets
                   .AsNoTracking()
                   .Where(t =>
                       t.IsDeleted == false
                       && t.Type == TweetType.Reply
                       && t.ParentTweetId == tweetId)
                   .Skip((page - 1) * pageSize)
                   .Take(pageSize)
                   .ToListAsync();
        }

        public async Task<Tweet> AddAsync(Tweet tweet)
        {
            ArgumentNullException.ThrowIfNull(tweet);
            await _context.Tweets.AddAsync(tweet);
            await _context.SaveChangesAsync();
            return tweet;
        }

        public async Task IncrementLikesCount(Guid tweetId)
        {
            var localTweet = await GetLocalTweet(tweetId);
            localTweet.LikesCount++;
            await _context.SaveChangesAsync();
        }

        public async Task DecrementLikesCount(Guid tweetId)
        {
            var localTweet = await GetLocalTweet(tweetId);
            localTweet.LikesCount--;
            await _context.SaveChangesAsync();
        }

        public async Task<Tweet> UpdateAsync(Tweet updatedTweet)
        {
            ArgumentNullException.ThrowIfNull(updatedTweet);
            var localTweet = await GetLocalTweet(updatedTweet.Id);
            _context.Entry(localTweet).CurrentValues.SetValues(updatedTweet);
            await _context.SaveChangesAsync();
            _context.Tweets.Entry(localTweet).State = EntityState.Detached;
            return localTweet;
        }

        public async Task UpdateRangeAsync(List<Tweet> tweets)
        {
            ArgumentNullException.ThrowIfNull(tweets);
            if (tweets.Count == 0)
                return;

            var ids = tweets.Select(t => t.Id).ToHashSet();
            var existingTweets = await _context.Tweets
                .Where(t =>
                    t.IsDeleted == false
                    && ids.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id);

            var missingIds = ids.Except(existingTweets.Keys);
            if (missingIds.Any())
                throw new KeyNotFoundException($"Tweets not found. Ids: {string.Join(", ", missingIds)}");

            foreach (var tweet in tweets)
            {
                if (existingTweets.TryGetValue(tweet.Id, out var existingTweet))
                    _context.Entry(existingTweet).CurrentValues.SetValues(tweet);
            }
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            await SoftDeleteRangeAsync([id]);
        }

        public async Task SoftDeleteRangeAsync(List<Guid> ids)
        {
            ArgumentNullException.ThrowIfNull(ids);
            if (ids.Count == 0)
                return;

            var idSet = new HashSet<Guid>(ids);
            var foundTweets = await _context.Tweets
                .Where(t => idSet.Contains(t.Id))
                .Select(t => new { t.Id, t.IsDeleted })
                .ToListAsync();

            var existingIds = foundTweets.Select(t => t.Id).ToList();
            var missingIds = idSet.Except(existingIds);
            if (missingIds.Any())
                throw new KeyNotFoundException($"Tweets not found: {string.Join(", ", missingIds)}");

            var alreadyDeletedIds = foundTweets
                .Where(t => t.IsDeleted == true)
                .Select(t => t.Id)
                .ToList();
            idSet.ExceptWith(alreadyDeletedIds);

            var allTweetIds = await GetAllTweetAndReplyIdsAsync(idSet);

            await _context.Tweets
                .Where(t => allTweetIds.Contains(t.Id))
                .ExecuteUpdateAsync(setters =>
                    setters
                        .SetProperty(t => t.IsDeleted, true)
                        .SetProperty(t => t.DeletedAt, DateTime.UtcNow)
                );
        }

        private async Task<HashSet<Guid>> GetAllTweetAndReplyIdsAsync(HashSet<Guid> tweetIds)
        {
            var allIds = new HashSet<Guid>(tweetIds);
            var queue = new Queue<Guid>(tweetIds);

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();

                var replyIds = await _context.Tweets
                    .Where(t =>
                        t.Type == TweetType.Reply
                        && t.ParentTweetId == currentId
                        && t.IsDeleted == false)
                    .Select(t => t.Id)
                    .ToListAsync();

                foreach (var replyId in replyIds)
                    if (allIds.Add(replyId))
                        queue.Enqueue(replyId);
            }
            return allIds;
        }

        private async Task<Tweet> GetLocalTweet(Guid id)
        {
            var localTweet = await _context.Tweets.FindAsync(id);

            if (localTweet == null)
                throw new KeyNotFoundException($"Tweet {id} not found.");
            if (localTweet.IsDeleted)
                throw new KeyNotFoundException($"Tweet {id} is deleted.");

            return localTweet;
        }
    }
}
