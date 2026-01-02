using Microsoft.EntityFrameworkCore;
using TweetService.Interfaces.Data;
using TweetService.Models;

namespace TweetService.Data
{
    public class TweetRepository : ITweetRepository
    {
        private const string ErrorMessage = "Db is unavailable";
        private readonly TweetDbContext _context;
        private readonly ILogger<TweetRepository> _logger;

        public TweetRepository(TweetDbContext tweetDbContext, ILogger<TweetRepository> logger)
        {
            _context = tweetDbContext;
            _logger = logger;
        }

        public async Task<Tweet?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Tweets
                       .AsNoTracking()
                       .Where(
                            t => t.Id.Equals(id)
                            && t.IsDeleted == false)
                       .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<List<Tweet>> GetByIdsAsync(List<Guid> ids)
        {
            try
            {
                return await _context.Tweets
                       .AsNoTracking()
                       .Where(t =>
                            t.IsDeleted == false
                            && ids.Contains(t.Id))
                       .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<List<Tweet>> GetByHashtagAsync(List<Guid> ids, string hashtag)
        {
            if (string.IsNullOrEmpty(hashtag))
                throw new ArgumentException(nameof(hashtag));
            try
            {
                return await _context.Tweets
                       .Include(t => t.TweetHashtags)
                       .AsNoTracking()
                       .Where(t =>
                            t.IsDeleted == false
                            && ids.Contains(t.Id)
                            && t.TweetHashtags.Any(th => th.Hashtag.Tag.Equals(hashtag)))
                       .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<List<Tweet>> GetByUserAsync(Guid userId)
        {
            try
            {
                return await _context.Tweets
                       .AsNoTracking()
                       .Where(t =>
                            t.IsDeleted == false
                            && t.AuthorId.Equals(userId))
                       .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<bool> IsRetweetedAsync(Guid tweetId, Guid userId)
        {
            try
            {
                return await _context.Tweets.AnyAsync(
                    t => t.AuthorId == userId
                    && t.Type == TweetType.Retweet
                    && t.ParentTweet!.Id == tweetId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<List<Guid>> GetRetweetedIdsAsync(List<Guid> tweetIds, Guid userId)
        {
            try
            {
                return await _context.Tweets.Where(
                    t => t.IsDeleted == false
                    && t.AuthorId == userId
                    && t.Type == TweetType.Retweet
                    && tweetIds.Contains(t.ParentTweet!.Id))
                    .Select(t => t.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<List<Tweet>> GetRepliesAsync(Guid tweetId)
        {
            try
            {
                return await _context.Tweets
                       .AsNoTracking()
                       .Where(t =>
                           t.IsDeleted == false
                           && t.Type == TweetType.Reply
                           && t.ParentTweetId == tweetId)
                       .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<Tweet> AddAsync(Tweet tweet)
        {
            ArgumentNullException.ThrowIfNull(tweet);
            try
            {
                await _context.Tweets.AddAsync(tweet);
                await _context.SaveChangesAsync();
                return tweet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<Tweet> UpdateAsync(Tweet tweet)
        {
            ArgumentNullException.ThrowIfNull(tweet);
            try
            {
                _context.Tweets.Update(tweet);
                await _context.SaveChangesAsync();
                return tweet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }

        public async Task<Tweet> DeleteAsync(Guid id)
        {
            try
            {
                var tweet = _context.Tweets
                    .AsNoTracking()
                    .Where(t => t.Id.Equals(id))
                    .FirstOrDefault();

                if (tweet == null)
                    throw new ArgumentException(nameof(id));

                tweet.IsDeleted = true;
                _context.Tweets.Update(tweet);
                await _context.SaveChangesAsync();
                return tweet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessage);
                throw new Exception(ErrorMessage, ex);
            }
        }
    }
}
