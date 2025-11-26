using Microsoft.EntityFrameworkCore;
using TweetService.Interfaces.Data;
using TweetService.Models;

namespace TweetService.Data
{
    public class HashtagRepository : IHashtagRepository
    {
        private readonly TweetDbContext _context;
        private readonly ILogger<HashtagRepository> _logger;

        public HashtagRepository(TweetDbContext tweetDbContext, ILogger<HashtagRepository> logger)
        {
            _context = tweetDbContext;
            _logger = logger;
        }

        public async Task<Hashtag?> GetById(Guid id)
        {
            try
            {
                return await _context.Hashtags
                       .AsNoTracking()
                       .Where(h => h.Id.Equals(id))
                       .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }

        public async Task<List<Hashtag>> Search(string query, int page, int pageSize)
        {
            var hashtags = await _context.Hashtags
                                    .AsNoTracking()
                                    .ToListAsync();

            if (string.IsNullOrEmpty(query) == false)
                hashtags = hashtags.Where(h => h.Tag.ToLower().Contains(query.ToLower())).ToList();

            return hashtags
                    .Skip(pageSize * (page - 1))
                    .Take(pageSize)
                    .ToList();
        }

        public async Task<Hashtag> Add(Hashtag hashtag)
        {
            ArgumentNullException.ThrowIfNull(hashtag);
            try
            {
                await _context.Hashtags.AddAsync(hashtag);
                await _context.SaveChangesAsync();
                return hashtag;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }

        public async Task<Hashtag> Update(Hashtag hashtag)
        {
            ArgumentNullException.ThrowIfNull(hashtag);
            try
            {
                _context.Hashtags.Update(hashtag);
                await _context.SaveChangesAsync();
                return hashtag;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }

        public async Task<Hashtag> Delete(Guid id)
        {
            try
            {
                var hashtag = _context.Hashtags
                    .AsNoTracking()
                    .Where(h => h.Id.Equals(id))
                    .FirstOrDefault();

                if (hashtag == null)
                    throw new ArgumentException(nameof(id));

                _context.Hashtags.Remove(hashtag);
                await _context.SaveChangesAsync();
                return hashtag;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }
    }
}
