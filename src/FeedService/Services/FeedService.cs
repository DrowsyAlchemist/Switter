using FeedService.DTOs;
using FeedService.Interfaces;
using FeedService.Interfaces.Data;
using FeedService.Interfaces.Infrastructure;

namespace FeedService.Services
{
    public class FeedService : IFeedService
    {
        private readonly IFeedRepository _feedRepository;
        private readonly IFeedBuilder _feedBuilder;
        private readonly ITweetServiceClient _tweetServiceClient;

        private readonly ILogger<FeedService> _logger;

        public FeedService(IFeedRepository feedRepository,
                          IFeedBuilder feedBuilder,
                          ITweetServiceClient tweetServiceClient,
                          ILogger<FeedService> logger)
        {
            _feedRepository = feedRepository;
            _feedBuilder = feedBuilder;
            _tweetServiceClient = tweetServiceClient;
            _logger = logger;
        }

        public async Task<FeedResponse> GetFeedAsync(Guid userId, FeedQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var startPosition = query.Cursor;
            var feedItems = await _feedRepository.GetFeedPageAsync(userId, startPosition, query.PageSize);

            if (feedItems.Count == 0)
            {
                await RebuildFeedAsync(userId);
                feedItems = await _feedRepository.GetFeedPageAsync(userId, 0, query.PageSize);
            }

            var orderedTweetIds = feedItems.Select(f => f.TweetId).ToList();
            var orderedTweetDtos = await GetTweetsByIdInSameOrderAsync(orderedTweetIds);

            var totalCount = await _feedRepository.GetFeedLengthAsync(userId);
            var hasMore = (startPosition + feedItems.Count) < totalCount;
            var nextCursor = hasMore
                ? (startPosition + feedItems.Count).ToString()
                : null;

            return new FeedResponse
            {
                Items = orderedTweetDtos,
                TotalCount = (int)totalCount,
                HasMore = hasMore,
                NextCursor = nextCursor
            };
        }

        public async Task RemoveFromFeedAsync(Guid userId, Guid tweetId)
        {
            await _feedRepository.RemoveFromFeedAsync(userId, tweetId);
        }

        public async Task RemoveUserTweetsFromFeedAsync(Guid feedOwnerId, Guid userToRemoveId)
        {
            await _feedRepository.RemoveUserTweetsFromFeedAsync(feedOwnerId, userToRemoveId);
        }

        public async Task RebuildFeedAsync(Guid userId)
        {
            await _feedBuilder.BuildFeedAsync(userId);
        }

        public async Task<int> GetFeedSizeAsync(Guid userId)
        {
            var length = await _feedRepository.GetFeedLengthAsync(userId);
            return (int)length;
        }

        private async Task<List<TweetDto>> GetTweetsByIdInSameOrderAsync(List<Guid> orderedIds)
        {
            var tweetDtos = await _tweetServiceClient.GetTweetsByIdAsync(orderedIds);

            return orderedIds
                .Join(tweetDtos,
                    id => id,
                    dto => dto.Id,
                    (id, dto) => dto)
                .ToList();
        }
    }
}
