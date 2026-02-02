using Microsoft.Extensions.Options;
using TweetService.DTOs;
using TweetService.Events;
using TweetService.Interfaces.Infrastructure;
using TweetService.Interfaces.Services;
using TweetService.Models.Options;

namespace TweetService.Services.Decorators.WithKafka
{
    public class LikeServiceWithKafka : ILikeService
    {
        private readonly ILikeService _likeService;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly KafkaOptions _options;
        private readonly ILogger<LikeServiceWithKafka> _logger;

        public LikeServiceWithKafka(
            ILikeService likeService,
            IKafkaProducer kafkaProducer,
            IOptions<KafkaOptions> options,
            ILogger<LikeServiceWithKafka> logger)
        {
            _likeService = likeService;
            _kafkaProducer = kafkaProducer;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<List<TweetDto>> GetLikedTweetsAsync(Guid userId, int page = 1, int pageSize = 20)
        {
            return await _likeService.GetLikedTweetsAsync(userId, page, pageSize);
        }

        public async Task LikeTweetAsync(Guid tweetId, Guid userId)
        {
            await _likeService.LikeTweetAsync(tweetId, userId);
            try
            {
                var likeSetEvent = new LikeSetEvent(userId, tweetId, DateTime.UtcNow);
                await _kafkaProducer.ProduceAsync(_options.TweetEvents.LikeSetEventName, likeSetEvent);
                _logger.LogInformation("Like-event message sent to kafka. \nUser: {}\nTweet: {}", userId, tweetId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Can't send like event to kafka.");
            }
        }

        public async Task UnlikeTweetAsync(Guid tweetId, Guid userId)
        {
            await _likeService.UnlikeTweetAsync(tweetId, userId);
            try
            {
                var likeCanceledEvent = new LikeCanceledEvent(userId, tweetId, DateTime.UtcNow);
                await _kafkaProducer.ProduceAsync(_options.TweetEvents.LikeCanceledEventName, likeCanceledEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Can't send unlike event to kafka.");
            }
        }
    }
}
