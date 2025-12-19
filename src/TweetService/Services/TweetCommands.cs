using AutoMapper;
using TweetService.DTOs;
using TweetService.Exceptions;
using TweetService.Interfaces.Data;
using TweetService.Interfaces.Infrastructure;
using TweetService.Interfaces.Services;
using TweetService.Models;

namespace TweetService.Services
{
    public class TweetCommands : ITweetCommands
    {
        private readonly ITweetRepository _tweetRepository;
        private readonly IMapper _mapper;
        private readonly IUserServiceClient _userServiceClient;
        private readonly IHashtagService _hashtagService;

        public TweetCommands(ITweetRepository tweetRepository,
            IMapper mapper,
            IUserServiceClient userServiceClient,
            IHashtagService hashtagService)
        {
            _tweetRepository = tweetRepository;
            _mapper = mapper;
            _userServiceClient = userServiceClient;
            _hashtagService = hashtagService;
        }

        public async Task<TweetDto> TweetAsync(Guid authorId, CreateTweetRequest request)
        {
            Tweet? parentTweet = null;
            if (request.Type == TweetType.Retweet || request.Type == TweetType.Reply)
                parentTweet = await TryGetParentTweetAsync(authorId, request.ParentTweetId);

            if (request.Type == TweetType.Retweet)
                if (parentTweet!.AuthorId == authorId)
                    throw new SelfRetweetException(parentTweet.Id, authorId);

            var author = await GetUserInfoAsync(authorId);

            var newTweet = new Tweet()
            {
                AuthorId = author.Id,
                AuthorDisplayName = author.DisplayName,
                AuthorAvatarUrl = author.AvatarUrl,
                Content = request.Content,
                Type = request.Type,
                ParentTweetId = request.ParentTweetId,
                CreatedAt = DateTime.UtcNow,
            };
            var tweet = await _tweetRepository.AddAsync(newTweet);

            await UpdateParentCountersAsync(request.Type, parentTweet!, 1);

            if (request.Type != TweetType.Reply && request.Content != string.Empty)
                await _hashtagService.ProcessHashtagsAsync(tweet.Id);

            return _mapper.Map<TweetDto>(tweet);
        }

        public async Task<TweetDto> DeleteTweetAsync(Guid tweetId, Guid userId)
        {
            var tweet = await _tweetRepository.GetByIdAsync(tweetId);
            if (tweet == null)
                throw new TweetNotFoundException(tweetId);
            if (tweet.AuthorId != userId)
                throw new DeleteTweetForbiddenException(tweetId, userId);

            var tweetDto = _mapper.Map<TweetDto>(tweet);

            await _tweetRepository.DeleteAsync(tweetId);

            if (tweet.Type == TweetType.Retweet || tweet.Type == TweetType.Reply)
            {
                var parentTweet = await TryGetParentTweetAsync(userId, tweet.ParentTweetId);
                await UpdateParentCountersAsync(tweet.Type, parentTweet, -1);
            }
            return tweetDto;
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

        private async Task<UserInfo> GetUserInfoAsync(Guid userId)
        {
            var userInfo = await _userServiceClient.GetUserInfoAsync(userId);
            if (userInfo == null)
                throw new UserNotFoundException(userId);
            return userInfo;
        }
    }
}