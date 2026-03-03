using TweetService.DTOs;
using TweetService.Interfaces.Services;
using TweetService.Models;

namespace TweetService.Services.Decorators
{
    public class TweetCommandsWithHashtags : ITweetCommands
    {
        private readonly ITweetCommands _tweetCommands;
        private readonly IHashtagService _hashtagService;

        public TweetCommandsWithHashtags(ITweetCommands tweetCommands, IHashtagService hashtagService)
        {
            _tweetCommands = tweetCommands;
            _hashtagService = hashtagService;
        }

        public async Task<TweetDto> TweetAsync(UserInfo authorInfo, CreateTweetRequest request)
        {
            var tweetDto = await _tweetCommands.TweetAsync(authorInfo, request);

            if (request.Type != TweetType.Reply)
                await _hashtagService.ProcessHashtagsAsync(tweetDto.Id, tweetDto.Content);

            return tweetDto;
        }

        public async Task DeleteTweetAsync(Guid tweetId, Guid userId)
        {
            await _tweetCommands.DeleteTweetAsync(tweetId, userId);
        }
    }
}
