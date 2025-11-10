using AutoMapper;
using Moq;
using UserService.DTOs;
using UserService.Events;
using UserService.Exceptions.Follows;
using UserService.Interfaces.Data;
using UserService.Interfaces;
using UserService.Models;
using UserService.Services;
using Xunit;
using UserService.Interfaces.Infrastructure;
using FluentAssertions;

namespace UserService.Tests.Unit
{
    public class FollowServiceTests
    {
        private readonly Mock<IFollowRepository> FollowRepositoryMock;
        private readonly Mock<IFollowersCounter> FollowersCounterMock;
        private readonly Mock<IKafkaProducerService> KafkaProducerMock;
        private readonly Mock<IMapper> MapperMock;
        private readonly FollowService FollowService;

        public FollowServiceTests()
        {
            FollowRepositoryMock = new Mock<IFollowRepository>();
            FollowersCounterMock = new Mock<IFollowersCounter>();
            KafkaProducerMock = new Mock<IKafkaProducerService>();
            MapperMock = new Mock<IMapper>();

            FollowService = new FollowService(
                FollowRepositoryMock.Object,
                FollowersCounterMock.Object,
                KafkaProducerMock.Object,
                MapperMock.Object
            );
        }

        [Fact]
        public async Task FollowUserAsync_WithValidUsers_CompletesSuccessfully()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            FollowRepositoryMock
                .Setup(x => x.IsFollowingAsync(followerId, followeeId))
                .ReturnsAsync(false);

            FollowRepositoryMock
                .Setup(x => x.AddAsync(followerId, followeeId))
                .ReturnsAsync(It.IsAny<Follow>());

            FollowersCounterMock
                .Setup(x => x.IncrementCounter(followerId, followeeId))
                .Returns(Task.CompletedTask);

            KafkaProducerMock
                .Setup(x => x.ProduceAsync("user-events", It.IsAny<UserFollowedEvent>()))
                .Returns(Task.CompletedTask);

            // Act
            await FollowService.FollowUserAsync(followerId, followeeId);

            // Assert
            FollowRepositoryMock.Verify(x => x.IsFollowingAsync(followerId, followeeId), Times.Once);
            FollowRepositoryMock.Verify(x => x.AddAsync(followerId, followeeId), Times.Once);
            FollowersCounterMock.Verify(x => x.IncrementCounter(followerId, followeeId), Times.Once);
            KafkaProducerMock.Verify(x => x.ProduceAsync("user-events", It.Is<UserFollowedEvent>(e =>
                e.FollowerId == followerId && e.FolloweeId == followeeId)), Times.Once);
        }

        [Fact]
        public async Task FollowUserAsync_WhenSelfFollow_ThrowsSelfFollowException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<SelfFollowException>(() =>
                FollowService.FollowUserAsync(userId, userId));

            FollowRepositoryMock.Verify(x => x.IsFollowingAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            FollowRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            FollowersCounterMock.Verify(x => x.IncrementCounter(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            KafkaProducerMock.Verify(x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<KafkaEvent>()), Times.Never);
        }

        [Fact]
        public async Task FollowUserAsync_WhenAlreadyFollowing_ThrowsDoubleFollowException()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            FollowRepositoryMock
                .Setup(x => x.IsFollowingAsync(followerId, followeeId))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<DoubleFollowException>(() =>
                FollowService.FollowUserAsync(followerId, followeeId));

            FollowRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            FollowersCounterMock.Verify(x => x.IncrementCounter(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            KafkaProducerMock.Verify(x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<KafkaEvent>()), Times.Never);
        }

        [Fact]
        public async Task UnfollowUserAsync_WithValidUsers_CompletesSuccessfully()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            FollowRepositoryMock
                .Setup(x => x.IsFollowingAsync(followerId, followeeId))
                .ReturnsAsync(true);

            FollowRepositoryMock
                .Setup(x => x.DeleteAsync(followerId, followeeId))
                .Returns(Task.CompletedTask);

            FollowersCounterMock
                .Setup(x => x.DecrementCounter(followerId, followeeId))
                .Returns(Task.CompletedTask);

            KafkaProducerMock
                .Setup(x => x.ProduceAsync("user-events", It.IsAny<UserUnfollowedEvent>()))
                .Returns(Task.CompletedTask);

            // Act
            await FollowService.UnfollowUserAsync(followerId, followeeId);

            // Assert
            FollowRepositoryMock.Verify(x => x.IsFollowingAsync(followerId, followeeId), Times.Once);
            FollowRepositoryMock.Verify(x => x.DeleteAsync(followerId, followeeId), Times.Once);
            FollowersCounterMock.Verify(x => x.DecrementCounter(followerId, followeeId), Times.Once);
            KafkaProducerMock.Verify(x => x.ProduceAsync("user-events", It.Is<UserUnfollowedEvent>(e =>
                e.FollowerId == followerId && e.FolloweeId == followeeId)), Times.Once);
        }

        [Fact]
        public async Task UnfollowUserAsync_WhenNotFollowing_ThrowsFollowNotFoundException()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            FollowRepositoryMock
                .Setup(x => x.IsFollowingAsync(followerId, followeeId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<FollowNotFoundException>(() =>
                FollowService.UnfollowUserAsync(followerId, followeeId));

            FollowRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            FollowersCounterMock.Verify(x => x.DecrementCounter(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            KafkaProducerMock.Verify(x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<KafkaEvent>()), Times.Never);
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

            FollowRepositoryMock
                .Setup(x => x.GetFollowersAsync(userId))
                .ReturnsAsync(allFollowers);

            MapperMock
                .Setup(x => x.Map<List<UserProfileDto>>(expectedPaginatedFollowers))
                .Returns(expectedDtos);

            // Act
            var result = await FollowService.GetFollowersAsync(userId, page, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedPaginatedFollowers.Count);
            result.Should().BeEquivalentTo(expectedDtos);

            FollowRepositoryMock.Verify(x => x.GetFollowersAsync(userId), Times.Once);
            MapperMock.Verify(x => x.Map<List<UserProfileDto>>(expectedPaginatedFollowers), Times.Once);
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

            FollowRepositoryMock
                .Setup(x => x.GetFollowersAsync(userId))
                .ReturnsAsync(allFollowers);

            MapperMock
                .Setup(x => x.Map<List<UserProfileDto>>(expectedPaginatedFollowers))
                .Returns(expectedDtos);

            // Act
            var result = await FollowService.GetFollowersAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            FollowRepositoryMock.Verify(x => x.GetFollowersAsync(userId), Times.Once);
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

            FollowRepositoryMock
                .Setup(x => x.GetFollowingsAsync(userId))
                .ReturnsAsync(allFollowing);

            MapperMock
                .Setup(x => x.Map<List<UserProfileDto>>(expectedPaginatedFollowing))
                .Returns(expectedDtos);

            // Act
            var result = await FollowService.GetFollowingAsync(userId, page, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedPaginatedFollowing.Count);
            result.Should().BeEquivalentTo(expectedDtos);

            FollowRepositoryMock.Verify(x => x.GetFollowingsAsync(userId), Times.Once);
            MapperMock.Verify(x => x.Map<List<UserProfileDto>>(expectedPaginatedFollowing), Times.Once);
        }

        [Fact]
        public async Task GetFollowingAsync_WithEmptyResults_ReturnsEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var emptyFollowing = new List<UserProfile>();
            var expectedDtos = new List<UserProfileDto>();

            FollowRepositoryMock
                .Setup(x => x.GetFollowingsAsync(userId))
                .ReturnsAsync(emptyFollowing);

            MapperMock
                .Setup(x => x.Map<List<UserProfileDto>>(emptyFollowing))
                .Returns(expectedDtos);

            // Act
            var result = await FollowService.GetFollowingAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            FollowRepositoryMock.Verify(x => x.GetFollowingsAsync(userId), Times.Once);
            MapperMock.Verify(x => x.Map<List<UserProfileDto>>(emptyFollowing), Times.Once);
        }

        [Fact]
        public async Task GetFollowersAsync_WithEmptyResults_ReturnsEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var emptyFollowers = new List<UserProfile>();
            var expectedDtos = new List<UserProfileDto>();

            FollowRepositoryMock
                .Setup(x => x.GetFollowersAsync(userId))
                .ReturnsAsync(emptyFollowers);

            MapperMock
                .Setup(x => x.Map<List<UserProfileDto>>(emptyFollowers))
                .Returns(expectedDtos);

            // Act
            var result = await FollowService.GetFollowersAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            FollowRepositoryMock.Verify(x => x.GetFollowersAsync(userId), Times.Once);
            MapperMock.Verify(x => x.Map<List<UserProfileDto>>(emptyFollowers), Times.Once);
        }
    }
}
