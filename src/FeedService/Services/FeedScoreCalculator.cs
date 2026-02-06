using FeedService.Interfaces;
using FeedService.Models.Options;
using Microsoft.Extensions.Options;

namespace FeedService.Services
{
    public class FeedScoreCalculator : IFeedScoreCalculator
    {
        private readonly double _likeWeight;
        private readonly double _retweetWeight;

        private readonly int _maxNoveltyFactor;
        private readonly int _noveltyFactorExpiryInHours;

        private readonly double _timeDecayFactor;

        public FeedScoreCalculator(IOptions<FeedOptions> options)
        {
            var scoreOptions = options.Value.Score;
            _likeWeight = scoreOptions.LikeWeight;
            _retweetWeight = scoreOptions.RetweetWeight;
            _maxNoveltyFactor = scoreOptions.MaxNoveltyFactor;
            _noveltyFactorExpiryInHours = scoreOptions.NoveltyFactorExpiryInHours;
            _timeDecayFactor = scoreOptions.TimeDecayFactor;
        }

        private const int HoursInDay = 24;

        public double Calculate(DateTime createdAt, int likes, int retweets)
        {
            var hoursSinceCreation = (DateTime.UtcNow - createdAt).TotalHours;
            var novelty = _maxNoveltyFactor * Math.Max(0, (1 - hoursSinceCreation / _noveltyFactorExpiryInHours));

            var engagement = likes * _likeWeight + retweets * _retweetWeight;

            var total = novelty + engagement;

            var timeCoefficient = hoursSinceCreation > _noveltyFactorExpiryInHours
                ? Math.Pow(_timeDecayFactor, (hoursSinceCreation - _noveltyFactorExpiryInHours) / HoursInDay)
                : 1.0;

            return total * timeCoefficient;
        }
    }
}
