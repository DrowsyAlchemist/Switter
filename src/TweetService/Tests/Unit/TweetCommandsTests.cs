using AutoMapper;
using FluentAssertions;
using Moq;
using TweetService.DTOs;
using TweetService.Exceptions;
using TweetService.Interfaces.Data;
using TweetService.Models;
using TweetService.Services;
using Xunit;

namespace TweetService.Tests.Unit
{
    public class TweetCommandsTests
    {
        private readonly Mock<ITweetRepository> _tweetRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly TweetCommands _tweetCommands;
        private readonly UserInfo _testUserInfo;
        private readonly DateTime _testDateTime;

        public TweetCommandsTests()
        {
            _tweetRepositoryMock = new Mock<ITweetRepository>();
            _mapperMock = new Mock<IMapper>();
            _tweetCommands = new TweetCommands(_tweetRepositoryMock.Object, _mapperMock.Object);

            _testUserInfo = new UserInfo
            {
                Id = Guid.NewGuid(),
                DisplayName = "Test User",
                AvatarUrl = "https://example.com/avatar.jpg"
            };

            _testDateTime = DateTime.UtcNow;
        }

        [Fact]
        public async Task TweetAsync_WithValidOriginalTweet_ReturnsTweetDto()
        {
            // Arrange
            var request = new CreateTweetRequest
            {
                Content = "Test tweet content",
                Type = TweetType.Tweet
            };

            var expectedTweet = new Tweet
            {
                Id = Guid.NewGuid(),
                AuthorId = _testUserInfo.Id,
                AuthorDisplayName = _testUserInfo.DisplayName,
                AuthorAvatarUrl = _testUserInfo.AvatarUrl,
                Content = request.Content,
                Type = request.Type,
                CreatedAt = _testDateTime
            };

            var expectedTweetDto = new TweetDto
            {
                AuthorId = expectedTweet.AuthorId,
                AuthorDisplayName = expectedTweet.AuthorDisplayName,
                Id = expectedTweet.Id,
                Content = expectedTweet.Content,
                Type = expectedTweet.Type
            };

            _tweetRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Tweet>()))
                .Callback<Tweet>(t => t.Id = expectedTweet.Id)
                .ReturnsAsync(expectedTweet);

            _mapperMock
                .Setup(x => x.Map<TweetDto>(expectedTweet))
                .Returns(expectedTweetDto);

            // Act
            var result = await _tweetCommands.TweetAsync(_testUserInfo, request);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedTweetDto);
            _tweetRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Tweet>()), Times.Once);
            _tweetRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Tweet>()), Times.Never);
        }

        [Fact]
        public async Task TweetAsync_WithValidReply_IncrementsParentRepliesCount()
        {
            // Arrange
            var parentTweetId = Guid.NewGuid();
            var request = new CreateTweetRequest
            {
                Content = "Reply content",
                Type = TweetType.Reply,
                ParentTweetId = parentTweetId
            };

            var parentTweet = new Tweet
            {
                Id = parentTweetId,
                Content = "ParentContent",
                AuthorId = Guid.NewGuid(),
                AuthorDisplayName = "TestUser",
                RepliesCount = 5,
                RetweetsCount = 2
            };

            var newTweet = new Tweet
            {
                Id = Guid.NewGuid(),
                AuthorId = _testUserInfo.Id,
                AuthorDisplayName = _testUserInfo.DisplayName,
                Content = request.Content,
                Type = request.Type,
                ParentTweetId = parentTweetId
            };

            _tweetRepositoryMock
                .Setup(x => x.GetByIdAsync(parentTweetId))
                .ReturnsAsync(parentTweet);

            _tweetRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Tweet>()))
                .Callback<Tweet>(t => t.Id = newTweet.Id)
                .ReturnsAsync(newTweet);

            _tweetRepositoryMock
                .Setup(x => x.UpdateAsync(parentTweet))
                .ReturnsAsync(It.IsAny<Tweet>());

            _mapperMock
                .Setup(x => x.Map<TweetDto>(It.IsAny<Tweet>()))
                .Returns(It.IsAny<TweetDto>());

            // Act
            await _tweetCommands.TweetAsync(_testUserInfo, request);

            // Assert
            parentTweet.RepliesCount.Should().Be(6);
            parentTweet.RetweetsCount.Should().Be(2);
            _tweetRepositoryMock.Verify(x => x.UpdateAsync(parentTweet), Times.Once);
        }

        [Fact]
        public async Task TweetAsync_WithValidRetweet_IncrementsParentRetweetsCount()
        {
            // Arrange
            var parentTweetId = Guid.NewGuid();
            var request = new CreateTweetRequest
            {
                Content = "Retweet content",
                Type = TweetType.Retweet,
                ParentTweetId = parentTweetId
            };

            var parentTweet = new Tweet
            {
                Id = parentTweetId,
                Content = "Content",
                AuthorId = Guid.NewGuid(),
                AuthorDisplayName = "ParentUser",
                RepliesCount = 3,
                RetweetsCount = 7
            };

            _tweetRepositoryMock
                .Setup(x => x.GetByIdAsync(parentTweetId))
                .ReturnsAsync(parentTweet);

            _tweetRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Tweet>()))
                .ReturnsAsync(It.IsAny<Tweet>());

            _tweetRepositoryMock
                .Setup(x => x.UpdateAsync(parentTweet))
                .ReturnsAsync(It.IsAny<Tweet>());

            _mapperMock
                .Setup(x => x.Map<TweetDto>(It.IsAny<Tweet>()))
                .Returns(It.IsAny<TweetDto>());

            // Act
            await _tweetCommands.TweetAsync(_testUserInfo, request);

            // Assert
            parentTweet.RetweetsCount.Should().Be(8);
            parentTweet.RepliesCount.Should().Be(3);
            _tweetRepositoryMock.Verify(x => x.UpdateAsync(parentTweet), Times.Once);
        }

        [Fact]
        public async Task TweetAsync_WithSelfRetweet_ThrowsSelfRetweetException()
        {
            // Arrange
            var parentTweetId = Guid.NewGuid();
            var request = new CreateTweetRequest
            {
                Content = "Retweet content",
                Type = TweetType.Retweet,
                ParentTweetId = parentTweetId
            };

            var parentTweet = new Tweet
            {
                Id = parentTweetId,
                Content = "Content",
                AuthorId = _testUserInfo.Id, // Same user tries to retweet their own tweet
                AuthorDisplayName = _testUserInfo.DisplayName,
                RepliesCount = 0,
                RetweetsCount = 0
            };

            _tweetRepositoryMock
                .Setup(x => x.GetByIdAsync(parentTweetId))
                .ReturnsAsync(parentTweet);

            // Act
            Func<Task> act = async () => await _tweetCommands.TweetAsync(_testUserInfo, request);

            // Assert
            await act.Should().ThrowAsync<SelfRetweetException>();
        }

        [Fact]
        public async Task TweetAsync_WithReplyAndNullParentTweetId_ThrowsParentTweetNullException()
        {
            // Arrange
            var request = new CreateTweetRequest
            {
                Content = "Reply content",
                Type = TweetType.Reply,
                ParentTweetId = null
            };

            // Act
            Func<Task> act = async () => await _tweetCommands.TweetAsync(_testUserInfo, request);

            // Assert
            await act.Should().ThrowAsync<ParentTweetNullException>();
        }

        [Fact]
        public async Task TweetAsync_WithRetweetAndNonExistentParentTweet_ThrowsTweetNotFoundException()
        {
            // Arrange
            var nonExistentTweetId = Guid.NewGuid();
            var request = new CreateTweetRequest
            {
                Content = "Retweet content",
                Type = TweetType.Retweet,
                ParentTweetId = nonExistentTweetId
            };

            _tweetRepositoryMock
                .Setup(x => x.GetByIdAsync(nonExistentTweetId))
                .ReturnsAsync((Tweet?)null);

            // Act
            Func<Task> act = async () => await _tweetCommands.TweetAsync(_testUserInfo, request);

            // Assert
            await act.Should().ThrowAsync<TweetNotFoundException>();
        }

        [Fact]
        public async Task DeleteTweetAsync_WithValidTweetAndOwner_SuccessfullyDeletes()
        {
            // Arrange
            var tweetId = Guid.NewGuid();
            var tweet = new Tweet
            {
                Id = tweetId,
                Content = "Content",
                AuthorId = _testUserInfo.Id,
                AuthorDisplayName = _testUserInfo.DisplayName,
                Type = TweetType.Tweet,
                ParentTweetId = null
            };

            _tweetRepositoryMock
                .Setup(x => x.GetByIdAsync(tweetId))
                .ReturnsAsync(tweet);

            _tweetRepositoryMock
                .Setup(x => x.SoftDeleteAsync(tweetId))
                .Returns(Task.CompletedTask);

            // Act
            await _tweetCommands.DeleteTweetAsync(tweetId, _testUserInfo.Id);

            // Assert
            _tweetRepositoryMock.Verify(x => x.GetByIdAsync(tweetId), Times.Once);
            _tweetRepositoryMock.Verify(x => x.SoftDeleteAsync(tweetId), Times.Once);
            _tweetRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Tweet>()), Times.Never);
        }

        [Fact]
        public async Task DeleteTweetAsync_WithReplyTweet_DecrementsParentRepliesCount()
        {
            // Arrange
            var tweetId = Guid.NewGuid();
            var parentTweetId = Guid.NewGuid();

            var parentTweet = new Tweet
            {
                Id = parentTweetId,
                Content = "Content",
                AuthorId = Guid.NewGuid(),
                AuthorDisplayName = "ParentUser",
                RepliesCount = 10,
                RetweetsCount = 5
            };

            var tweet = new Tweet
            {
                Id = tweetId,
                Content = "Content",
                AuthorId = _testUserInfo.Id,
                AuthorDisplayName = _testUserInfo.DisplayName,
                Type = TweetType.Reply,
                ParentTweetId = parentTweetId
            };

            _tweetRepositoryMock
                .Setup(x => x.GetByIdAsync(tweetId))
                .ReturnsAsync(tweet);

            _tweetRepositoryMock
                .Setup(x => x.GetByIdAsync(parentTweetId))
                .ReturnsAsync(parentTweet);

            _tweetRepositoryMock
                .Setup(x => x.SoftDeleteAsync(tweetId))
                .Returns(Task.CompletedTask);

            _tweetRepositoryMock
                .Setup(x => x.UpdateAsync(parentTweet))
                .ReturnsAsync(It.IsAny<Tweet>());

            // Act
            await _tweetCommands.DeleteTweetAsync(tweetId, _testUserInfo.Id);

            // Assert
            parentTweet.RepliesCount.Should().Be(9);
            parentTweet.RetweetsCount.Should().Be(5);
            _tweetRepositoryMock.Verify(x => x.UpdateAsync(parentTweet), Times.Once);
        }

        [Fact]
        public async Task DeleteTweetAsync_WithRetweetTweet_DecrementsParentRetweetsCount()
        {
            // Arrange
            var tweetId = Guid.NewGuid();
            var parentTweetId = Guid.NewGuid();

            var parentTweet = new Tweet
            {
                Id = parentTweetId,
                Content = "Content",
                AuthorId = Guid.NewGuid(),
                AuthorDisplayName = "ParentUser",
                RepliesCount = 3,
                RetweetsCount = 15
            };

            var tweet = new Tweet
            {
                Id = tweetId,
                Content = "Content",
                AuthorId = _testUserInfo.Id,
                AuthorDisplayName = _testUserInfo.DisplayName,
                Type = TweetType.Retweet,
                ParentTweetId = parentTweetId
            };

            _tweetRepositoryMock
                .Setup(x => x.GetByIdAsync(tweetId))
                .ReturnsAsync(tweet);

            _tweetRepositoryMock
                .Setup(x => x.GetByIdAsync(parentTweetId))
                .ReturnsAsync(parentTweet);

            _tweetRepositoryMock
                .Setup(x => x.SoftDeleteAsync(tweetId))
                .Returns(Task.CompletedTask);

            _tweetRepositoryMock
                .Setup(x => x.UpdateAsync(parentTweet))
                .ReturnsAsync(It.IsAny<Tweet>());

            // Act
            await _tweetCommands.DeleteTweetAsync(tweetId, _testUserInfo.Id);

            // Assert
            parentTweet.RetweetsCount.Should().Be(14);
            parentTweet.RepliesCount.Should().Be(3);
            _tweetRepositoryMock.Verify(x => x.UpdateAsync(parentTweet), Times.Once);
        }

        [Fact]
        public async Task DeleteTweetAsync_WithNonExistentTweet_ThrowsTweetNotFoundException()
        {
            // Arrange
            var nonExistentTweetId = Guid.NewGuid();

            _tweetRepositoryMock
                .Setup(x => x.GetByIdAsync(nonExistentTweetId))
                .ReturnsAsync((Tweet?)null);

            // Act
            Func<Task> act = async () => await _tweetCommands.DeleteTweetAsync(nonExistentTweetId, _testUserInfo.Id);

            // Assert
            await act.Should()
                .ThrowAsync<TweetNotFoundException>();
        }

        [Fact]
        public async Task DeleteTweetAsync_WithInvalidAuthor_ThrowsDeleteTweetForbiddenException()
        {
            // Arrange
            var tweetId = Guid.NewGuid();
            var tweetOwnerId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            var tweet = new Tweet
            {
                Id = tweetId,
                Content = "Content",
                AuthorId = tweetOwnerId,
                AuthorDisplayName = "TweetAuthor",
                Type = TweetType.Tweet,
            };

            _tweetRepositoryMock
                .Setup(x => x.GetByIdAsync(tweetId))
                .ReturnsAsync(tweet);

            // Act
            Func<Task> act = async () => await _tweetCommands.DeleteTweetAsync(tweetId, otherUserId);

            // Assert
            await act.Should()
                .ThrowAsync<DeleteTweetForbiddenException>();
        }
    }
}