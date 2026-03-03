using Microsoft.Extensions.Options;
using TweetService.DTOs;
using TweetService.Events;
using TweetService.Interfaces.Infrastructure;
using TweetService.Interfaces.Services;
using TweetService.Models.Options;

namespace TweetService.Services.Decorators.WithKafka
{
    public class TweetCommandsWithKafka : ITweetCommands
    {
        private readonly ITweetCommands _tweetCommands;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly KafkaOptions _options;
        private readonly ILogger<TweetCommandsWithKafka> _logger;

        public TweetCommandsWithKafka(
            ITweetCommands tweetCommands,
            IOptions<KafkaOptions> options,
            IKafkaProducer kafkaProducer,
            ILogger<TweetCommandsWithKafka> logger)
        {
            _tweetCommands = tweetCommands;
            _options = options.Value;
            _kafkaProducer = kafkaProducer;
            _logger = logger;
        }

        public async Task<TweetDto> TweetAsync(UserInfo author, CreateTweetRequest request)
        {
            var tweet = await _tweetCommands.TweetAsync(author, request);
            try
            {
                var tweetEvent = new TweetCreatedEvent(tweet.Id, tweet.AuthorId, tweet.Type, tweet.CreatedAt);
                await _kafkaProducer.ProduceAsync(_options.TweetEvents.TweetCreatedEventName, tweetEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Can't send tweet event to kafka.");
            }
            return tweet;
        }

        public async Task DeleteTweetAsync(Guid tweetId, Guid userId)
        {
            await _tweetCommands.DeleteTweetAsync(tweetId, userId);
            try
            {
                var tweetEvent = new TweetDeletedEvent(tweetId, DateTime.UtcNow);
                await _kafkaProducer.ProduceAsync(_options.TweetEvents.TweetDeletedEventName, tweetEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Can't send tweet event to kafka.");
            }
        }
    }
}
