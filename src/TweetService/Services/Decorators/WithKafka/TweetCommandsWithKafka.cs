using TweetService.DTOs;
using TweetService.Events;
using TweetService.Interfaces.Infrastructure;
using TweetService.Interfaces.Services;

namespace TweetService.Services.Decorators.WithKafka
{
    public class TweetCommandsWithKafka : ITweetCommands
    {
        private readonly ITweetCommands _tweetCommands;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly ILogger<TweetCommandsWithKafka> _logger;

        public TweetCommandsWithKafka(ITweetCommands tweetCommands, IKafkaProducer kafkaProducer, ILogger<TweetCommandsWithKafka> logger)
        {
            _tweetCommands = tweetCommands;
            _kafkaProducer = kafkaProducer;
            _logger = logger;
        }

        public async Task<TweetDto> TweetAsync(UserInfo author, CreateTweetRequest request)
        {
            var tweet = await _tweetCommands.TweetAsync(author, request);
            try
            {
                var tweetEvent = new TweetCreatedEvent(tweet.Id, tweet.AuthorId, tweet.Type, tweet.CreatedAt);
                await _kafkaProducer.ProduceAsync("tweet-created", tweetEvent);
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
                await _kafkaProducer.ProduceAsync("tweet-deleted", tweetEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Can't send tweet event to kafka.");
            }
        }
    }
}
