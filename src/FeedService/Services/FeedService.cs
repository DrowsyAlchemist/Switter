using FeedService.DTOs;
using FeedService.Interfaces;
using FeedService.Interfaces.Data;
using FeedService.Interfaces.Infrastructure;
using FeedService.Models;

namespace FeedService.Services
{
    public class FeedService : IFeedService
    {
        private readonly IFeedRepository _feedRepository;
        private readonly IFeedBuilder _feedBuilder;

        private readonly ITweetServiceClient _tweetServiceClient;
        private readonly IProfileServiceClient _profileServiceClient;
        private readonly ILogger<FeedService> _logger;

        public FeedService(IFeedRepository feedRepository,
                          ITweetServiceClient tweetServiceClient,
                          IProfileServiceClient profileServiceClient,
                          ILogger<FeedService> logger)
        {
            _feedRepository = feedRepository;
            _tweetServiceClient = tweetServiceClient;
            _profileServiceClient = profileServiceClient;
            _logger = logger;
        }

        public async Task<FeedResponse> GetFeedAsync(Guid userId, FeedQuery query)
        {
            // 1. Получаем FeedItems из Redis
            var start = GetStartPosition(query.Cursor);
            var feedItems = await _feedRepository.GetFeedPageAsync(userId, start, query.PageSize);

            // Если лента пуста, возможно нужно ее построить
            if (feedItems.Any() == false)
            {
                await RebuildFeedAsync(userId);
                feedItems = await _feedRepository.GetFeedPageAsync(userId, 0, query.PageSize);
            }

            // 2. Получаем информацию о твитах в порядке как в FeedItems
            var orderedTweetIds = feedItems.Select(f => f.TweetId).ToList();
            var tweetDtos = await _tweetServiceClient.GetTweetsByIds(orderedTweetIds);

            List<TweetDto> orderedDtos = orderedTweetIds
                .Join(tweetDtos,
                    id => id,
                    dto => dto.Id,
                    (id, dto) => dto)
                .ToList();

            // 4. Формируем ответ с пагинацией
            var totalCount = await _feedRepository.GetFeedLengthAsync(userId);
            var nextCursor = feedItems.Count > 0
                ? (start + feedItems.Count).ToString()
                : null;

            return new FeedResponse
            {
                Items = orderedDtos,
                TotalCount = (int)totalCount,
                HasMore = start + feedItems.Count < totalCount,
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

        private int GetStartPosition(string? cursor)
        {
            if (string.IsNullOrEmpty(cursor) || !int.TryParse(cursor, out var position))
                return 0;

            return Math.Max(0, position);
        }
    }
}
