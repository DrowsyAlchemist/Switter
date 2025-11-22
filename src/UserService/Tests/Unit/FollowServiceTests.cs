using AutoMapper;
using Moq;
using UserService.DTOs;
using UserService.Exceptions.Follows;
using UserService.Interfaces.Data;
using UserService.Interfaces;
using UserService.Models;
using UserService.Services;
using Xunit;
using UserService.Interfaces.Infrastructure;
using FluentAssertions;
using UserService.KafkaEvents.UserEvents;

namespace UserService.Tests.Unit
{
    public class FollowServiceTests
    {
        private readonly Mock<IFollowRepository> _followRepositoryMock;
        private readonly Mock<IFollowersCounter> _followersCounterMock;
        private readonly Mock<IKafkaProducer> _kafkaProducerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly FollowService _followService;

        public FollowServiceTests()
        {
            _followRepositoryMock = new Mock<IFollowRepository>();
            _followersCounterMock = new Mock<IFollowersCounter>();
            _kafkaProducerMock = new Mock<IKafkaProducer>();
            _mapperMock = new Mock<IMapper>();

            //_followService = new FollowService(
            //    _followRepositoryMock.Object,
            //    _followersCounterMock.Object,
            //    _kafkaProducerMock.Object,
            //    _mapperMock.Object
            //);
        }

        [Fact]
        public async Task FollowUserAsync_WithValidUsers_CompletesSuccessfully()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            _followRepositoryMock
                .Setup(x => x.IsFollowingAsync(followerId, followeeId))
                .ReturnsAsync(false);

            _followRepositoryMock
                .Setup(x => x.AddAsync(followerId, followeeId))
                .ReturnsAsync(It.IsAny<Follow>());

            _followersCounterMock
                .Setup(x => x.IncrementCounter(followerId, followeeId))
                .Returns(Task.CompletedTask);

            _kafkaProducerMock
                .Setup(x => x.ProduceAsync("user-events", It.IsAny<UserFollowedEvent>()))
                .Returns(Task.CompletedTask);

            // Act
            await _followService.FollowUserAsync(followerId, followeeId);

            // Assert
            _followRepositoryMock.Verify(x => x.IsFollowingAsync(followerId, followeeId), Times.Once);
            _followRepositoryMock.Verify(x => x.AddAsync(followerId, followeeId), Times.Once);
            _followersCounterMock.Verify(x => x.IncrementCounter(followerId, followeeId), Times.Once);
            _kafkaProducerMock.Verify(x => x.ProduceAsync("user-events", It.Is<UserFollowedEvent>(e =>
                e.FollowerId == followerId && e.FolloweeId == followeeId)), Times.Once);
        }

        [Fact]
        public async Task FollowUserAsync_WhenSelfFollow_ThrowsSelfFollowException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<SelfFollowException>(() =>
                _followService.FollowUserAsync(userId, userId));

            _followRepositoryMock.Verify(x => x.IsFollowingAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _followRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _followersCounterMock.Verify(x => x.IncrementCounter(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _kafkaProducerMock.Verify(x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<UserFollowedEvent>()), Times.Never);
        }

        [Fact]
        public async Task FollowUserAsync_WhenAlreadyFollowing_ThrowsDoubleFollowException()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            _followRepositoryMock
                .Setup(x => x.IsFollowingAsync(followerId, followeeId))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<DoubleFollowException>(() =>
                _followService.FollowUserAsync(followerId, followeeId));

            _followRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _followersCounterMock.Verify(x => x.IncrementCounter(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _kafkaProducerMock.Verify(x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<UserFollowedEvent>()), Times.Never);
        }

        [Fact]
        public async Task UnfollowUserAsync_WithValidUsers_CompletesSuccessfully()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            _followRepositoryMock
                .Setup(x => x.IsFollowingAsync(followerId, followeeId))
                .ReturnsAsync(true);

            _followRepositoryMock
                .Setup(x => x.DeleteAsync(followerId, followeeId))
                .Returns(Task.CompletedTask);

            _followersCounterMock
                .Setup(x => x.DecrementCounter(followerId, followeeId))
                .Returns(Task.CompletedTask);

            _kafkaProducerMock
                .Setup(x => x.ProduceAsync("user-events", It.IsAny<UserUnfollowedEvent>()))
                .Returns(Task.CompletedTask);

            // Act
            await _followService.UnfollowUserAsync(followerId, followeeId);

            // Assert
            _followRepositoryMock.Verify(x => x.IsFollowingAsync(followerId, followeeId), Times.Once);
            _followRepositoryMock.Verify(x => x.DeleteAsync(followerId, followeeId), Times.Once);
            _followersCounterMock.Verify(x => x.DecrementCounter(followerId, followeeId), Times.Once);
            _kafkaProducerMock.Verify(x => x.ProduceAsync("user-events", It.Is<UserUnfollowedEvent>(e =>
                e.FollowerId == followerId && e.FolloweeId == followeeId)), Times.Once);
        }

        [Fact]
        public async Task UnfollowUserAsync_WhenNotFollowing_ThrowsFollowNotFoundException()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            _followRepositoryMock
                .Setup(x => x.IsFollowingAsync(followerId, followeeId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<FollowNotFoundException>(() =>
                _followService.UnfollowUserAsync(followerId, followeeId));

            _followRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _followersCounterMock.Verify(x => x.DecrementCounter(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetFollowersAsync_WithValidUserId_ReturnsPaginatedFollowers()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var page = 2;
            var pageSize = 10;

            var allFollowers = new List<UserProfile>
            {
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Follower1" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Follower2" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Follower3" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Follower4" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Follower5" }
            };

            var expectedPaginatedFollowers = allFollowers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var expectedDtos = expectedPaginatedFollowers
                .Select(f => new UserProfileDto { Id = f.Id, DisplayName = f.DisplayName })
                .ToList();

            _followRepositoryMock
                .Setup(x => x.GetFollowersAsync(userId))
                .ReturnsAsync(allFollowers);

            _mapperMock
                .Setup(x => x.Map<List<UserProfileDto>>(expectedPaginatedFollowers))
                .Returns(expectedDtos);

            // Act
            var result = await _followService.GetFollowersAsync(userId, page, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedPaginatedFollowers.Count);
            result.Should().BeEquivalentTo(expectedDtos);

            _followRepositoryMock.Verify(x => x.GetFollowersAsync(userId), Times.Once);
            _mapperMock.Verify(x => x.Map<List<UserProfileDto>>(expectedPaginatedFollowers), Times.Once);
        }

        [Fact]
        public async Task GetFollowersAsync_WithDefaultParameters_UsesDefaultValues()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var allFollowers = new List<UserProfile>
            {
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Follower1" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Follower2" }
            };

            var expectedPaginatedFollowers = allFollowers
                .Skip(0)
                .Take(20)
                .ToList();

            var expectedDtos = expectedPaginatedFollowers
                .Select(f => new UserProfileDto { Id = f.Id, DisplayName = f.DisplayName })
                .ToList();

            _followRepositoryMock
                .Setup(x => x.GetFollowersAsync(userId))
                .ReturnsAsync(allFollowers);

            _mapperMock
                .Setup(x => x.Map<List<UserProfileDto>>(expectedPaginatedFollowers))
                .Returns(expectedDtos);

            // Act
            var result = await _followService.GetFollowersAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            _followRepositoryMock.Verify(x => x.GetFollowersAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetFollowingAsync_WithValidUserId_ReturnsPaginatedFollowing()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var page = 1;
            var pageSize = 5;

            var allFollowing = new List<UserProfile>
            {
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Following1" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Following2" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Following3" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Following4" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Following5" }
            };

            var expectedPaginatedFollowing = allFollowing
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var expectedDtos = expectedPaginatedFollowing
                .Select(f => new UserProfileDto { Id = f.Id, DisplayName = f.DisplayName })
                .ToList();

            _followRepositoryMock
                .Setup(x => x.GetFollowingsAsync(userId))
                .ReturnsAsync(allFollowing);

            _mapperMock
                .Setup(x => x.Map<List<UserProfileDto>>(expectedPaginatedFollowing))
                .Returns(expectedDtos);

            // Act
            var result = await _followService.GetFollowingAsync(userId, page, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedPaginatedFollowing.Count);
            result.Should().BeEquivalentTo(expectedDtos);

            _followRepositoryMock.Verify(x => x.GetFollowingsAsync(userId), Times.Once);
            _mapperMock.Verify(x => x.Map<List<UserProfileDto>>(expectedPaginatedFollowing), Times.Once);
        }

        [Fact]
        public async Task GetFollowingAsync_WithEmptyResults_ReturnsEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var emptyFollowing = new List<UserProfile>();
            var expectedDtos = new List<UserProfileDto>();

            _followRepositoryMock
                .Setup(x => x.GetFollowingsAsync(userId))
                .ReturnsAsync(emptyFollowing);

            _mapperMock
                .Setup(x => x.Map<List<UserProfileDto>>(emptyFollowing))
                .Returns(expectedDtos);

            // Act
            var result = await _followService.GetFollowingAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _followRepositoryMock.Verify(x => x.GetFollowingsAsync(userId), Times.Once);
            _mapperMock.Verify(x => x.Map<List<UserProfileDto>>(emptyFollowing), Times.Once);
        }

        [Fact]
        public async Task GetFollowersAsync_WithEmptyResults_ReturnsEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var emptyFollowers = new List<UserProfile>();
            var expectedDtos = new List<UserProfileDto>();

            _followRepositoryMock
                .Setup(x => x.GetFollowersAsync(userId))
                .ReturnsAsync(emptyFollowers);

            _mapperMock
                .Setup(x => x.Map<List<UserProfileDto>>(emptyFollowers))
                .Returns(expectedDtos);

            // Act
            var result = await _followService.GetFollowersAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _followRepositoryMock.Verify(x => x.GetFollowersAsync(userId), Times.Once);
            _mapperMock.Verify(x => x.Map<List<UserProfileDto>>(emptyFollowers), Times.Once);
        }

        [Fact]
        public async Task IsFollowing_WhenFollowing_ReturnsTrue()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            _followRepositoryMock
                .Setup(x => x.IsFollowingAsync(followerId, followeeId))
                .ReturnsAsync(true);

            // Act
            var result = await _followService.IsFollowingAsync(followerId, followeeId);

            // Assert
            result.Should().BeTrue();
            _followRepositoryMock.Verify(x => x.IsFollowingAsync(followerId, followeeId), Times.Once);
        }

        [Fact]
        public async Task IsFollowing_WhenNotFollowing_ReturnsFalse()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            _followRepositoryMock
                .Setup(x => x.IsFollowingAsync(followerId, followeeId))
                .ReturnsAsync(false);

            // Act
            var result = await _followService.IsFollowingAsync(followerId, followeeId);

            // Assert
            result.Should().BeFalse();
            _followRepositoryMock.Verify(x => x.IsFollowingAsync(followerId, followeeId), Times.Once);
        }

        [Fact]
        public async Task IsBlocked_WithSameUser_ReturnsFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _followRepositoryMock
                .Setup(x => x.IsFollowingAsync(userId, userId))
                .ReturnsAsync(false);

            // Act
            var result = await _followService.IsFollowingAsync(userId, userId);

            // Assert
            result.Should().BeFalse();
            _followRepositoryMock.Verify(x => x.IsFollowingAsync(userId, userId), Times.Once);
        }
    }
}
