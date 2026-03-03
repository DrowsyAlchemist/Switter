using FluentAssertions;
using Moq;
using Xunit;
using TweetService.DTOs;
using TweetService.Exceptions;
using TweetService.Models;
using AutoMapper;
using TweetService.Services;
using TweetService.Interfaces.Data.Repositories;

namespace TweetService.Tests.Unit
{
    public class LikeServiceTests
    {
        private readonly Mock<ILikesRepository> _likesRepositoryMock;
        private readonly Mock<ITweetRepository> _tweetRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly LikeService _likeService;

        public LikeServiceTests()
        {
            _likesRepositoryMock = new Mock<ILikesRepository>();
            _tweetRepositoryMock = new Mock<ITweetRepository>();
            _mapperMock = new Mock<IMapper>();

            _likeService = new LikeService(
                _likesRepositoryMock.Object,
                _tweetRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        public class GetLikedTweetsAsyncTests : LikeServiceTests
        {
            [Fact]
            public async Task GetLikedTweetsAsync_NoLikedTweets_ReturnsEmptyList()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var emptyIdList = new List<Guid>();
                var emptyTweetList = new List<Tweet>();
                var emptyDtoList = new List<TweetDto>();

                _likesRepositoryMock.Setup(r => r.GetLikedTweetIdsAsync(userId, 1, 10))
                    .ReturnsAsync(emptyIdList);
                _tweetRepositoryMock.Setup(r => r.GetByIdsAsync(emptyIdList))
                    .ReturnsAsync(emptyTweetList);
                _mapperMock.Setup(m => m.Map<List<TweetDto>>(emptyTweetList))
                    .Returns(emptyDtoList);

                // Act
                var result = await _likeService.GetLikedTweetsAsync(userId, 1, 10);

                // Assert
                result.Should().BeEmpty();
            }
        }

        public class LikeTweetAsyncTests : LikeServiceTests
        {
            [Fact]
            public async Task LikeTweetAsync_TweetNotLiked_SuccessfullyLikes()
            {
                // Arrange
                var tweetId = Guid.NewGuid();
                var userId = Guid.NewGuid();
                var tweet = new Tweet
                {
                    AuthorId = Guid.NewGuid(),
                    AuthorDisplayName = "TestAuthor1",
                    Id = tweetId,
                    Content = "Test tweet",
                    LikesCount = 5
                };
                var like = new Like { Id = Guid.NewGuid(), TweetId = tweetId, UserId = userId };

                _likesRepositoryMock.Setup(r => r.IsExistAsync(tweetId, userId))
                    .ReturnsAsync(false);

                _tweetRepositoryMock.Setup(r => r.GetByIdAsync(tweetId))
                    .ReturnsAsync(tweet);

                _likesRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Like>()))
                    .ReturnsAsync(like);

                // Act
                await _likeService.LikeTweetAsync(tweetId, userId);

                // Assert
                _likesRepositoryMock.Verify(r => r.IsExistAsync(tweetId, userId), Times.Once);
                _tweetRepositoryMock.Verify(r => r.GetByIdAsync(tweetId), Times.Once);
                _likesRepositoryMock.Verify(r => r.AddAsync(It.Is<Like>(l =>
                    l.TweetId == tweetId && l.UserId == userId)), Times.Once);
                _tweetRepositoryMock.Verify(r => r.IncrementLikesCount(tweetId), Times.Once);
            }

            [Fact]
            public async Task LikeTweetAsync_TweetAlreadyLiked_ThrowsDoubleLikeException()
            {
                // Arrange
                var tweetId = Guid.NewGuid();
                var userId = Guid.NewGuid();

                _likesRepositoryMock.Setup(r => r.IsExistAsync(tweetId, userId))
                    .ReturnsAsync(true);

                // Act
                Func<Task> act = async () => await _likeService.LikeTweetAsync(tweetId, userId);

                // Assert
                await act.Should().ThrowAsync<DoubleLikeException>();
            }

            [Fact]
            public async Task LikeTweetAsync_TweetNotFound_ThrowsTweetNotFoundException()
            {
                // Arrange
                var tweetId = Guid.NewGuid();
                var userId = Guid.NewGuid();

                _likesRepositoryMock.Setup(r => r.IsExistAsync(tweetId, userId))
                    .ReturnsAsync(false);

                _tweetRepositoryMock.Setup(r => r.GetByIdAsync(tweetId))
                    .ReturnsAsync((Tweet)null);

                // Act
                Func<Task> act = async () => await _likeService.LikeTweetAsync(tweetId, userId);

                // Assert
                await act.Should().ThrowAsync<TweetNotFoundException>();
                _likesRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Like>()), Times.Never);
            }

            [Fact]
            public async Task LikeTweetAsync_ExceptionDuringSave_RollsBackTransaction()
            {
                // Arrange
                var tweetId = Guid.NewGuid();
                var userId = Guid.NewGuid();
                var tweet = new Tweet { AuthorId = Guid.NewGuid(), AuthorDisplayName = "TestAuthor1", Id = tweetId, Content = "Content" };

                _likesRepositoryMock.Setup(r => r.IsExistAsync(tweetId, userId))
                    .ReturnsAsync(false);

                _tweetRepositoryMock.Setup(r => r.GetByIdAsync(tweetId))
                    .ReturnsAsync(tweet);

                _tweetRepositoryMock.Setup(r => r.IncrementLikesCount(tweetId))
                    .ThrowsAsync(new Exception("Database error"));

                // Act
                Func<Task> act = async () => await _likeService.LikeTweetAsync(tweetId, userId);

                // Assert
                await act.Should().ThrowAsync<Exception>();
            }
        }

        public class UnlikeTweetAsyncTests : LikeServiceTests
        {
            [Fact]
            public async Task UnlikeTweetAsync_TweetLiked_SuccessfullyUnlikes()
            {
                // Arrange
                var tweetId = Guid.NewGuid();
                var userId = Guid.NewGuid();
                var like = new Like { Id = Guid.NewGuid(), TweetId = tweetId, UserId = userId };
                var tweet = new Tweet
                {
                    AuthorId = Guid.NewGuid(),
                    AuthorDisplayName = "TestAuthor1",
                    Id = tweetId,
                    Content = "Test tweet",
                    LikesCount = 10
                };

                _likesRepositoryMock.Setup(r => r.GetAsync(tweetId, userId))
                    .ReturnsAsync(like);

                _tweetRepositoryMock.Setup(r => r.GetByIdAsync(tweetId))
                    .ReturnsAsync(tweet);

                // Act
                await _likeService.UnlikeTweetAsync(tweetId, userId);

                // Assert
                _likesRepositoryMock.Verify(r => r.GetAsync(tweetId, userId), Times.Once);
                _tweetRepositoryMock.Verify(r => r.GetByIdAsync(tweetId), Times.Once);
                _likesRepositoryMock.Verify(r => r.DeleteAsync(like.Id), Times.Once);
                _tweetRepositoryMock.Verify(r => r.DecrementLikesCount(tweetId), Times.Once);
            }

            [Fact]
            public async Task UnlikeTweetAsync_TweetNotLiked_ThrowsLikeNotFoundException()
            {
                // Arrange
                var tweetId = Guid.NewGuid();
                var userId = Guid.NewGuid();

                _likesRepositoryMock.Setup(r => r.GetAsync(tweetId, userId))
                    .ReturnsAsync((Like)null);

                // Act
                Func<Task> act = async () => await _likeService.UnlikeTweetAsync(tweetId, userId);

                // Assert
                await act.Should().ThrowAsync<LikeNotFoundException>();
                _tweetRepositoryMock.Verify(r => r.GetByIdAsync(tweetId), Times.Never);
            }

            [Fact]
            public async Task UnlikeTweetAsync_TweetNotFound_ThrowsTweetNotFoundException()
            {
                // Arrange
                var tweetId = Guid.NewGuid();
                var userId = Guid.NewGuid();
                var like = new Like { Id = Guid.NewGuid(), TweetId = tweetId, UserId = userId };

                _likesRepositoryMock.Setup(r => r.GetAsync(tweetId, userId))
                    .ReturnsAsync(like);

                _tweetRepositoryMock.Setup(r => r.GetByIdAsync(tweetId))
                    .ReturnsAsync((Tweet)null);

                // Act
                Func<Task> act = async () => await _likeService.UnlikeTweetAsync(tweetId, userId);

                // Assert
                await act.Should().ThrowAsync<TweetNotFoundException>();
                _likesRepositoryMock.Verify(r => r.DeleteAsync(like.Id), Times.Never);
            }

            [Fact]
            public async Task UnlikeTweetAsync_ExceptionDuringDelete_RollsBackTransaction()
            {
                // Arrange
                var tweetId = Guid.NewGuid();
                var userId = Guid.NewGuid();
                var like = new Like { Id = Guid.NewGuid(), TweetId = tweetId, UserId = userId };
                var tweet = new Tweet { AuthorId = Guid.NewGuid(), AuthorDisplayName = "TestAuthor1", Id = tweetId, Content = "Content" };

                _likesRepositoryMock.Setup(r => r.GetAsync(tweetId, userId))
                    .ReturnsAsync(like);

                _tweetRepositoryMock.Setup(r => r.GetByIdAsync(tweetId))
                    .ReturnsAsync(tweet);

                _likesRepositoryMock.Setup(r => r.DeleteAsync(like.Id))
                    .ThrowsAsync(new Exception("Database error"));

                // Act
                Func<Task> act = async () => await _likeService.UnlikeTweetAsync(tweetId, userId);

                // Assert
                await act.Should().ThrowAsync<Exception>();
            }
        }
    }
}