using TweetService.Data;
using TweetService.DTOs;
using TweetService.Interfaces.Services;
using TweetService.Models;

namespace TweetService.Services.Decorators
{
    public class TweetCommandsWithHashtags : ITweetCommands
    {
        private readonly ITweetCommands _tweetCommands;
        private readonly IHashtagService _hashtagService;
        private readonly TweetDbContext _context;

        public TweetCommandsWithHashtags(
            ITweetCommands tweetCommands,
            IHashtagService hashtagService,
            TweetDbContext context)
        {
            _tweetCommands = tweetCommands;
            _hashtagService = hashtagService;
            _context = context;
        }

        public async Task<TweetDto> TweetAsync(UserInfo authorInfo, CreateTweetRequest request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var tweetDto = await _tweetCommands.TweetAsync(authorInfo, request);

                if (request.Type != TweetType.Reply)
                    await _hashtagService.ProcessHashtagsAsync(tweetDto.Id, tweetDto.Content);

                return tweetDto;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteTweetAsync(Guid tweetId, Guid userId)
        {
            await _tweetCommands.DeleteTweetAsync(tweetId, userId);
        }
    }
}
