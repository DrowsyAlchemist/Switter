using FeedService.Models.Options;
using FeedService.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FeedService.Tests.Unit
{
    public class FeedScoreCalculatorTests
    {
        private readonly Mock<IOptions<FeedOptions>> _optionsMock = new();
        private FeedScoreCalculator _calculator;

        public FeedScoreCalculatorTests()
        {
            var scoreOptions = new ScoreOptions
            {
                LikeWeight = 1.0,
                RetweetWeight = 2.0,
                MaxNoveltyFactor = 10,
                NoveltyFactorExpiryInHours = 24,
                TimeDecayFactor = 0.95
            };

            var feedOptions = new FeedOptions { Score = scoreOptions };
            _optionsMock.Setup(o => o.Value).Returns(feedOptions);

            _calculator = new FeedScoreCalculator(_optionsMock.Object);
        }

        [Fact]
        public void Calculate_NewTweetWithLikes_ShouldHaveHigherScore_ThanOldTweetWithSameLikes()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var newTweetTime = now.AddHours(-1); 
            var oldTweetTime = now.AddHours(-48);

            var likes = 10;
            var retweets = 5;

            // Act
            var newTweetScore = _calculator.Calculate(newTweetTime, likes, retweets);
            var oldTweetScore = _calculator.Calculate(oldTweetTime, likes, retweets);

            // Assert
            newTweetScore.Should().BeGreaterThan(oldTweetScore);
        }

        [Fact]
        public void Calculate_TweetWithEngagement_ShouldHaveHigherScore_ThanTweetWithoutEngagement_WhenSameAge()
        {
            // Arrange
            var tweetTime = DateTime.UtcNow.AddHours(-12);

            // Act
            var scoreWithEngagement = _calculator.Calculate(tweetTime, likes: 10, retweets: 5);
            var scoreWithoutEngagement = _calculator.Calculate(tweetTime, likes: 0, retweets: 0);

            // Assert
            scoreWithEngagement.Should().BeGreaterThan(scoreWithoutEngagement);
        }

        [Fact]
        public void Calculate_Retweets_ShouldHaveMoreWeight_ThanLikes_WhenRetweetWeightGreater()
        {
            // Arrange
            var tweetTime = DateTime.UtcNow.AddHours(-6);

            // Act
            var scoreWithRetweets = _calculator.Calculate(tweetTime, likes: 0, retweets: 3);
            var scoreWithLikes = _calculator.Calculate(tweetTime, likes: 6, retweets: 0);

            // Assert
            scoreWithRetweets.Should().BeApproximately(scoreWithLikes, 0.001);
        }

        [Fact]
        public void Calculate_NoveltyFactor_ShouldDecrease()
        {
            // Arrange
            var now = DateTime.UtcNow;

            // Act
            var score1h = _calculator.Calculate(now.AddHours(-1), 0, 0);
            var score12h = _calculator.Calculate(now.AddHours(-12), 0, 0);
            var score23h = _calculator.Calculate(now.AddHours(-23), 0, 0);

            // Assert
            score1h.Should().BeGreaterThan(score12h);
            score12h.Should().BeGreaterThan(score23h);
        }

        [Fact]
        public void Calculate_TimeDecay_ShouldApply_After24Hours()
        {
            // Arrange
            var now = DateTime.UtcNow;

            // Act
            var score25h = _calculator.Calculate(now.AddHours(-25), likes: 100, retweets: 50);
            var score49h = _calculator.Calculate(now.AddHours(-49), likes: 100, retweets: 50);

            // Assert
            score25h.Should().BeGreaterThan(score49h);
        }

        [Fact]
        public void Calculate_VeryOldTweet_ShouldHaveVeryLowScore_EvenWithManyLikes()
        {
            // Arrange
            var now = DateTime.UtcNow;

            var veryOldTweet = _calculator.Calculate(now.AddDays(-90), likes: 1000, retweets: 500);
            var newTweet = _calculator.Calculate(now.AddHours(-1), likes: 10, retweets: 5);

            // Assert
            newTweet.Should().BeGreaterThan(veryOldTweet);
        }
    }
}
