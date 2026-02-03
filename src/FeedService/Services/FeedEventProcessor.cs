using FeedService.Events;
using FeedService.Interfaces;
using FeedService.Interfaces.Infrastructure;

namespace FeedService.Services
{
    public class FeedEventProcessor : IFeedEventProcessor
    {
        private const int FeedItemsCountForFollowers = 30;
        private const int FollowersCacheTtlInMinutes = 10;

        private readonly IFeedFiller _feedFiller;
        private readonly IProfileServiceClient _profileServiceClient;
        private readonly ITweetServiceClient _tweetServiceClient;
        private readonly ILogger<FeedEventProcessor> _logger;

        private readonly Dictionary<Guid, List<Guid>> _followersCache = new();
        private readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(FollowersCacheTtlInMinutes);

        public FeedEventProcessor(
            IFeedFiller feedFiller,
            IProfileServiceClient userServiceClient,
            ITweetServiceClient tweetServiceClient,
            ILogger<FeedEventProcessor> logger)
        {
            _feedFiller = feedFiller;
            _profileServiceClient = userServiceClient;
            _tweetServiceClient = tweetServiceClient;
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

            var recentTweets = await GetRecentTweetsAsync(followEvent.FolloweeId, FeedItemsCountForFollowers);
            await _feedFiller.AddTweetToFeedsAsync(followEvent.FollowerId, recentTweets);

            _followersCache[followEvent.FolloweeId].Add(followEvent.FollowerId);
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

            _followersCache[following].Remove(follower);

            await _feedFiller.RemoveUserTweetsFromFeed(feedOwnerId: follower, userToRemoveId: following);
        }

        public async Task ProcessUserBlockedAsync(UserBlockedEvent userBlockedEvent)
        {
            ArgumentNullException.ThrowIfNull(userBlockedEvent);

            var blocker = userBlockedEvent.BlockerId;
            var blocked = userBlockedEvent.BlockedId;

            _followersCache[blocked].Remove(blocker);

            await _feedFiller.RemoveUserTweetsFromFeed(
                feedOwnerId: blocker,
                userToRemoveId: blocked);
        }

        private async Task AddToFollowersFeed(Guid following, Guid tweetId)
        {
            var followers = await GetFollowersAsync(following);
            await _feedFiller.AddTweetToFeedsAsync(tweetId: tweetId, userIds: followers);
        }

        private async Task<List<Guid>> GetFollowersAsync(Guid userId)
        {
            if (_followersCache.TryGetValue(userId, out var cachedFollowers))
                return cachedFollowers;

            try
            {
                var followers = await _profileServiceClient.GetFollowersAsync(userId);

                _followersCache[userId] = followers;

                _ = Task.Delay(_cacheTtl).ContinueWith(_ =>
                    _followersCache.Remove(userId));

                return followers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get followers for user {UserId}", userId);
                return new List<Guid>();
            }
        }

        private async Task<List<Guid>> GetRecentTweetsAsync(Guid userId, int count)
        {
            try
            {
                return await _tweetServiceClient.GetRecentTweetsAsync(userId, count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get recent tweets for user {UserId}", userId);
                return new List<Guid>();
            }
        }
    }
}
