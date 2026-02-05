using FeedService.Interfaces;
using FeedService.Interfaces.Data;
using FeedService.Interfaces.Infrastructure;
using FeedService.Models;

namespace FeedService.Services
{
    public class FeedFiller : IFeedFiller
    {
        private readonly IFeedRepository _feedRepository;
        private readonly ITweetServiceClient _tweetServiceClient;
        private readonly IFeedScoreCalculator _scoreCalculator;

        public FeedFiller(IFeedRepository feedRepository, ITweetServiceClient tweetServiceClient, IFeedScoreCalculator scoreCalculator)
        {
            _feedRepository = feedRepository;
            _tweetServiceClient = tweetServiceClient;
            _scoreCalculator = scoreCalculator;
        }

        public async Task AddTweetToFeedAsync(Guid tweetId, Guid userId)
        {
            var feedItem = await CreateFeedItemsAsync([tweetId]);
            await _feedRepository.AddToFeedAsync(userId, feedItem);
        }

        public async Task AddTweetsToFeedAsync(List<Guid> tweetIds, Guid userId)
        {
            ArgumentNullException.ThrowIfNull(tweetIds);
            if (tweetIds.Count == 0)
                return;

            var feedItems = await CreateFeedItemsAsync(tweetIds);
            await _feedRepository.AddToFeedAsync(userId, feedItems);
        }

        public async Task AddTweetToFeedsAsync(Guid tweetId, IEnumerable<Guid> userIds)
        {
            ArgumentNullException.ThrowIfNull(userIds);
            if (userIds.Any() == false)
                return;

            var feedItem = await CreateFeedItemsAsync([tweetId]);

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

        private async Task<List<FeedItem>> CreateFeedItemsAsync(List<Guid> tweetIds, Guid userId)
        {
            var tweetDtos = await _tweetServiceClient.GetTweetsByIdAsync(tweetIds);

            HashSet<Guid> blockedUsers = (await _profileServiceClient.GetBlocked(userId)).ToHashSet();
            tweetDtos = tweetDtos.Where(dto => blockedUsers.Contains(dto.AuthorId) == false).ToList();

            var feedItems = new List<FeedItem>();

            foreach (var tweetDto in tweetDtos)
            {
                var feedItemScore = _scoreCalculator.Calculate(tweetDto!.CreatedAt, tweetDto.LikesCount, tweetDto.RetweetsCount);
                var feedItem = new FeedItem
                {
                    TweetId = tweetDto.Id,
                    AuthorId = tweetDto.AuthorId,
                    Score = feedItemScore
                };
                feedItems.Add(feedItem);
            }
            return feedItems;
        }
    }
}