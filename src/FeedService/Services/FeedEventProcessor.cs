using FeedService.Events;
using FeedService.Interfaces;
using FeedService.Interfaces.Data;
using FeedService.Interfaces.Infrastructure;
using FeedService.Models.Options;
using Microsoft.Extensions.Options;

namespace FeedService.Services
{
    public class FeedEventProcessor : IFeedEventProcessor
    {
        private readonly IFeedFiller _feedFiller;
        private readonly IFollowsRepository _followsRepository;
        private readonly ITweetServiceClient _tweetServiceClient;
        private readonly FeedOptions _options;
        private readonly ILogger<FeedEventProcessor> _logger;

        public FeedEventProcessor(
            IFeedFiller feedFiller,
            IFollowsRepository followsRepository,
            ITweetServiceClient tweetServiceClient,

            IOptions<FeedOptions> options,
            ILogger<FeedEventProcessor> logger)
        {
            _feedFiller = feedFiller;
            _followsRepository = followsRepository;
            _tweetServiceClient = tweetServiceClient;

            _options = options.Value;
            _logger = logger;
        }

        public async Task ProcessTweetCreatedAsync(TweetCreatedEvent tweetEvent)
        {
            ArgumentNullException.ThrowIfNull(tweetEvent);
            await _feedFiller.AddTweetToFeedAsync(tweetId: tweetEvent.Id, userId: tweetEvent.AuthorId);
            await AddToFollowersFeed(tweetEvent.AuthorId, tweetEvent.Id);
        }

        public async Task ProcessRetweetAsync(RetweetCreatedEvent retweetEvent)
        {
            ArgumentNullException.ThrowIfNull(retweetEvent);
            await AddToFollowersFeed(retweetEvent.AuthorId, retweetEvent.ParentTweet); // Add only parent original tweet
        }

        public async Task ProcessUserFollowedAsync(UserFollowedEvent followEvent)
        {
            ArgumentNullException.ThrowIfNull(followEvent);

            var follower = followEvent.FollowerId;
            var following = followEvent.FolloweeId;

            var recentTweets = await GetRecentTweetsAsync(following, _options.TweetsByEachFollowingMaxCount);
            await _feedFiller.AddTweetsToFeedAsync(recentTweets, follower);

            await _followsRepository.AddFollowerAsync(follower, following);
        }

        public async Task ProcessTweetLikedAsync(LikeSetEvent likeEvent)
        {
            ArgumentNullException.ThrowIfNull(likeEvent);
            await AddToFollowersFeed(likeEvent.UserId, likeEvent.TweetId);
        }

        public async Task ProcessUserUnfollowedAsync(UserUnfollowedEvent userUnfollowedEvent)
        {
            ArgumentNullException.ThrowIfNull(userUnfollowedEvent);

            var follower = userUnfollowedEvent.FollowerId;
            var following = userUnfollowedEvent.FolloweeId;

            await _feedFiller.RemoveUserTweetsFromFeedAsync(feedOwnerId: follower, userToRemoveId: following);

            await _followsRepository.RemoveFollowerAsync(follower, following);
        }

        public async Task ProcessUserBlockedAsync(UserBlockedEvent userBlockedEvent)
        {
            ArgumentNullException.ThrowIfNull(userBlockedEvent);

            var blocker = userBlockedEvent.BlockerId;
            var blocked = userBlockedEvent.BlockedId;

            await _feedFiller.RemoveUserTweetsFromFeedAsync(feedOwnerId: blocker, userToRemoveId: blocked);

            await _followsRepository.RemoveFollowerAsync(blocker, blocked);
        }

        private async Task AddToFollowersFeed(Guid following, Guid tweetId)
        {
            var followers = await _followsRepository.GetFollowersAsync(following);
            await _feedFiller.AddTweetToFeedsAsync(tweetId: tweetId, userIds: followers);
        }

        private async Task<List<Guid>> GetRecentTweetsAsync(Guid userId, int count)
        {
            try
            {
                return await _tweetServiceClient.GetRecentUserTweetIdsAsync(userId, count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get recent tweets for user {UserId}", userId);
                return new List<Guid>();
            }
        }
    }
}
