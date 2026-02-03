using FeedService.Interfaces;
using FeedService.Interfaces.Data;
using FeedService.Interfaces.Infrastructure;

namespace FeedService.Services
{
    public class FeedBuilder : IFeedBuilder
    {
        private readonly IFeedFiller _filler;
        private readonly IFeedRepository _feedRepository;
        private readonly IScoreCalculator _scoreCalculator;

        private readonly ITweetServiceClient _tweetServiceClient;
        private readonly IProfileServiceClient _profileServiceClient;

        private readonly ILogger<FeedBuilder> _logger;

        public async Task BuildFeedAsync(Guid userId)
        {
            try
            {
                await _feedRepository.ClearFeedAsync(userId);

                var followingIds = await _profileServiceClient.GetFollowingAsync(userId);

                foreach (var followingId in followingIds)
                {
                    var recentTweets = await _tweetServiceClient.GetRecentTweetsAsync(followingId, 30);
                    await _filler.AddTweetsToFeedAsync(recentTweets, userId);
                }

                // Добавить тренды!
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rebuilding feed for user {UserId}", userId);
                throw;
            }
        }
    }
}
