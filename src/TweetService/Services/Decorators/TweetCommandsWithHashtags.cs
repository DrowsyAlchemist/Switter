using TweetService.DTOs;
using TweetService.Interfaces.Data;
using TweetService.Interfaces.Services;
using TweetService.Models;

namespace TweetService.Services.Decorators
{
    public class TweetCommandsWithHashtags : ITweetCommands
    {
        private readonly ITweetCommands _tweetCommands;
        private readonly IHashtagService _hashtagService;
        private readonly ITransactionManager _transactionManager;

        public TweetCommandsWithHashtags(
            ITweetCommands tweetCommands,
            IHashtagService hashtagService,
            ITransactionManager transactionManager)
        {
            _tweetCommands = tweetCommands;
            _hashtagService = hashtagService;
            _transactionManager = transactionManager;
        }

        public async Task<TweetDto> TweetAsync(UserInfo authorInfo, CreateTweetRequest request)
        {
            await using var transaction = await _transactionManager.BeginTransactionAsync();

            try
            {
                var tweetDto = await _tweetCommands.TweetAsync(authorInfo, request);

                if (request.Type != TweetType.Reply)
                    await _hashtagService.ProcessHashtagsAsync(tweetDto.Id, tweetDto.Content);

                await transaction.CommitAsync();
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
