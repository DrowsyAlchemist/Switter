using FeedService.DTOs;
using FeedService.Interfaces.Data;
using FeedService.Interfaces.Infrastructure;
using FeedService.Interfaces;
using FeedService.Models;
using Moq;
using Xunit;
using FluentAssertions;

namespace FeedService.Tests.Unit
{
    public class FeedServiceTests
    {
        private readonly Mock<IFeedRepository> _feedRepositoryMock = new();
        private readonly Mock<IFeedBuilder> _feedBuilderMock = new();
        private readonly Mock<ITweetServiceClient> _tweetServiceClientMock = new();
        private readonly Mock<ILogger<Services.FeedService>> _loggerMock = new();
        private readonly Services.FeedService _feedService;

        public FeedServiceTests()
        {
            _feedService = new Services.FeedService(
                _feedRepositoryMock.Object,
                _feedBuilderMock.Object,
                _tweetServiceClientMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task GetFeedAsync_ShouldPreserveOrderFromRepository_WhenTweetsReturnedInDifferentOrder()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new FeedQuery { PageSize = 3, Cursor = 0 };

            // Feed items from repository in specific order
            var feedItems = new List<FeedItem>
            {
                new FeedItem { TweetId = Guid.Parse("11111111-1111-1111-1111-111111111111"), AuthorId=Guid.Empty,Score=0 },
                new FeedItem { TweetId = Guid.Parse("22222222-2222-2222-2222-222222222222"), AuthorId=Guid.Empty,Score=0  },
                new FeedItem { TweetId = Guid.Parse("33333333-3333-3333-3333-333333333333"), AuthorId=Guid.Empty,Score=0  }
            };

            // TweetService returns tweets in DIFFERENT order
            var tweetDtos = new List<TweetDto>
            {
                new TweetDto { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Content = "Third",
                    AuthorDisplayName="Author",
                    AuthorId=Guid.Empty,
                    Type = TweetType.Tweet},
                new TweetDto { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Content = "First",
                    AuthorDisplayName="Author",
                    AuthorId=Guid.Empty,
                    Type = TweetType.Tweet },
                new TweetDto { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Content = "Second",
                    AuthorDisplayName = "Author",
                    AuthorId = Guid.Empty,
                    Type = TweetType.Tweet
                },
            };

            _feedRepositoryMock
                .Setup(r => r.GetFeedPageAsync(userId, 0, 3))
                .ReturnsAsync(feedItems);

            _feedRepositoryMock
                .Setup(r => r.GetFeedLengthAsync(userId))
                .ReturnsAsync(10);

            _tweetServiceClientMock
                .Setup(c => c.GetTweetsByIdAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(tweetDtos);

            // Act
            var result = await _feedService.GetFeedAsync(userId, query);

            // Assert
            result.Items.Should().HaveCount(3);
            result.Items[0].Id.Should().Be(feedItems[0].TweetId);
            result.Items[1].Id.Should().Be(feedItems[1].TweetId);
            result.Items[2].Id.Should().Be(feedItems[2].TweetId);
        }

        [Fact]
        public async Task GetFeedAsync_ShouldReturnEmptyList_WhenNoFeedItemsAndNoTweets()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new FeedQuery { PageSize = 10, Cursor = 0 };

            _feedRepositoryMock
                .SetupSequence(r => r.GetFeedPageAsync(userId, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<FeedItem>())
                .ReturnsAsync(new List<FeedItem>());

            _feedRepositoryMock
                .Setup(r => r.GetFeedLengthAsync(userId))
                .ReturnsAsync(0);

            _tweetServiceClientMock
                .Setup(c => c.GetTweetsByIdAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(new List<TweetDto>());

            // Act
            var result = await _feedService.GetFeedAsync(userId, query);

            // Assert
            result.Items.Should().BeEmpty();
            _feedBuilderMock.Verify(b => b.BuildFeedAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetFeedAsync_ShouldHandlePartialTweetsReturn_WhenSomeTweetsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var query = new FeedQuery { PageSize = 4, Cursor = 0 };

            var feedItems = new List<FeedItem>
            {
                new FeedItem { TweetId = Guid.Parse("11111111-1111-1111-1111-111111111111"), AuthorId=Guid.Empty,Score=0 },
                new FeedItem { TweetId = Guid.Parse("22222222-2222-2222-2222-222222222222"), AuthorId=Guid.Empty,Score=0  },
                new FeedItem { TweetId = Guid.Parse("33333333-3333-3333-3333-333333333333"), AuthorId=Guid.Empty,Score=0  },
                new FeedItem { TweetId = Guid.Parse("44444444-4444-4444-4444-444444444444"), AuthorId=Guid.Empty,Score=0  }
            };

            var tweetDtos = new List<TweetDto>
            {
                    new TweetDto { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Content = "Second",
                    AuthorDisplayName = "Author",
                    AuthorId = Guid.Empty,
                    Type = TweetType.Tweet
                    },
                    new TweetDto { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Content = "Second",
                    AuthorDisplayName = "Author",
                    AuthorId = Guid.Empty,
                    Type = TweetType.Tweet
                    },
            };

            _feedRepositoryMock
                .Setup(r => r.GetFeedPageAsync(userId, 0, 4))
                .ReturnsAsync(feedItems);

            _feedRepositoryMock
                .Setup(r => r.GetFeedLengthAsync(userId))
                .ReturnsAsync(10);

            _tweetServiceClientMock
                .Setup(c => c.GetTweetsByIdAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(tweetDtos);

            // Act
            var result = await _feedService.GetFeedAsync(userId, query);

            // Assert
            result.Items.Should().HaveCount(2); 
            result.Items[0].Id.Should().Be(Guid.Parse("22222222-2222-2222-2222-222222222222"));
            result.Items[1].Id.Should().Be(Guid.Parse("44444444-4444-4444-4444-444444444444"));
        }
    }
}