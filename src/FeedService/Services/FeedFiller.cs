using FeedService.Interfaces;
using FeedService.Interfaces.Data;
using FeedService.Models;

namespace FeedService.Services
{
    public class FeedFiller : IFeedFiller
    {
        private readonly IFeedRepository _feedRepository;
        private readonly IScoreCalculator _scoreCalculator;

        public FeedFiller(IFeedRepository feedRepository, IScoreCalculator scoreCalculator)
        {
            _feedRepository = feedRepository;
            _scoreCalculator = scoreCalculator;
        }

        public async Task AddTweetToFeedAsync(Guid tweetId, Guid userId)
        {
            var feedItem = await CreateFeedItemAsync(tweetId);
            await _feedRepository.AddToFeedAsync(userId, feedItem);
        }

        public async Task AddTweetsToFeedAsync(List<Guid> tweetIds, Guid userId)
        {
            ArgumentNullException.ThrowIfNull(tweetIds);
            if (tweetIds.Any() == false)
                return;

            var feedItems = new List<FeedItem>();
            foreach (var tweetId in tweetIds)
            {
                var feedItem = await CreateFeedItemAsync(tweetId);
                feedItems.Add(feedItem);
            }
            await _feedRepository.AddToFeedAsync(userId, feedItems);
        }

        public async Task AddTweetToFeedsAsync(Guid tweetId, IEnumerable<Guid> userIds)
        {
            ArgumentNullException.ThrowIfNull(userIds);
            if (userIds.Any() == false)
                return;

            var feedItem = await CreateFeedItemAsync(tweetId);

            foreach (var userId in userIds)
                await _feedRepository.AddToFeedAsync(userId, feedItem);
        }

        public async Task RemoveUserTweetsFromFeedAsync(Guid feedOwnerId, Guid userToRemoveId)
        {
            await _feedRepository.RemoveUserTweetsFromFeedAsync(feedOwnerId, userToRemoveId);
        }

        public async Task ClearFeedAsync(Guid userId)
        {
            await _feedRepository.ClearFeedAsync(userId);
        }

        private async Task<FeedItem> CreateFeedItemAsync(Guid tweetId)
        {
            var feedScore = await _scoreCalculator.CalculateAsync(tweetId);
            var feedItem = new FeedItem
            {
                TweetId = tweetId,
                Score = feedScore
            };
            return feedItem;
        }
    }
}