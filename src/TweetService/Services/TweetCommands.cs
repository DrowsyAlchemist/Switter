using AutoMapper;
using TweetService.DTOs;
using TweetService.Exceptions;
using TweetService.Interfaces.Data;
using TweetService.Interfaces.Services;
using TweetService.Models;

namespace TweetService.Services
{
    public class TweetCommands : ITweetCommands
    {
        private readonly ITweetRepository _tweetRepository;
        private readonly IMapper _mapper;

        public TweetCommands(ITweetRepository tweetRepository, IMapper mapper)
        {
            _tweetRepository = tweetRepository;
            _mapper = mapper;
        }

        public async Task<TweetDto> TweetAsync(UserInfo authorInfo, CreateTweetRequest request)
        {
            Tweet? parentTweet = null;
            if (request.Type == TweetType.Retweet || request.Type == TweetType.Reply)
                parentTweet = await TryGetParentTweetAsync(authorInfo.Id, request.ParentTweetId);

            if (request.Type == TweetType.Retweet)
                if (parentTweet!.AuthorId == authorInfo.Id)
                    throw new SelfRetweetException(parentTweet.Id, authorInfo.Id);

            var newTweet = new Tweet()
            {
                AuthorId = authorInfo.Id,
                AuthorDisplayName = authorInfo.DisplayName,
                AuthorAvatarUrl = authorInfo.AvatarUrl,
                Content = request.Content,
                Type = request.Type,
                ParentTweetId = request.ParentTweetId,
                CreatedAt = DateTime.UtcNow,
            };
            var tweet = await _tweetRepository.AddAsync(newTweet);

            if (parentTweet != null)
                await UpdateParentCountersAsync(request.Type, parentTweet, 1);

            return _mapper.Map<TweetDto>(tweet);
        }

        public async Task DeleteTweetAsync(Guid tweetId, Guid userId)
        {
            var tweet = await _tweetRepository.GetByIdAsync(tweetId);
            if (tweet == null)
                throw new TweetNotFoundException(tweetId);
            if (tweet.AuthorId != userId)
                throw new DeleteTweetForbiddenException(tweetId, userId);

            await _tweetRepository.SoftDeleteAsync(tweetId);

            if (tweet.Type == TweetType.Retweet || tweet.Type == TweetType.Reply)
            {
                var parentTweet = await TryGetParentTweetAsync(userId, tweet.ParentTweetId);
                await UpdateParentCountersAsync(tweet.Type, parentTweet, -1);
            }
        }

        private async Task<Tweet> TryGetParentTweetAsync(Guid authorId, Guid? parentTweetId)
        {
            if (parentTweetId == null)
                throw new ParentTweetNullException();

            var parentTweet = await _tweetRepository.GetByIdAsync(parentTweetId.Value);
            if (parentTweet == null)
                throw new TweetNotFoundException(parentTweetId.Value);

            return parentTweet;
        }

        private async Task UpdateParentCountersAsync(TweetType tweetType, Tweet parentTweet, int value)
        {

            if (tweetType == TweetType.Retweet)
                parentTweet.RetweetsCount += value;
            else if (tweetType == TweetType.Reply)
                parentTweet.RepliesCount += value;

            await _tweetRepository.UpdateAsync(parentTweet);
        }
    }
}