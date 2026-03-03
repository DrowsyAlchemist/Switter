#if DEBUG
using FluentAssertions;
using Moq;
using Xunit;
using UserService.Exceptions.Follows;
using UserService.Interfaces;
using UserService.Services.Commands;
using UserService.Interfaces.Data;

namespace UserService.Tests.Unit.Commands
{
    public class FollowCommandsTests
    {
        private readonly Mock<IFollowRepository> _mockFollowRepository;
        private readonly Mock<IUserRelationshipService> _mockRelationshipService;
        private readonly FollowCommands _followCommands;

        public FollowCommandsTests()
        {
            _mockFollowRepository = new Mock<IFollowRepository>();
            _mockRelationshipService = new Mock<IUserRelationshipService>();
            _followCommands = new FollowCommands(_mockFollowRepository.Object, _mockRelationshipService.Object);
        }

        [Fact]
        public async Task FollowUserAsync_WithDifferentUserIdsAndNoBlocks_CallsAddAsync()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            _mockFollowRepository
                .Setup(repo => repo.IsFollowingAsync(followerId, followeeId))
                .ReturnsAsync(false);

            _mockRelationshipService
                .Setup(service => service.IsBlockedAsync(followerId, followeeId))
                .ReturnsAsync(false);

            _mockRelationshipService
                .Setup(service => service.IsBlockedAsync(followeeId, followerId))
                .ReturnsAsync(false);

            // Act
            await _followCommands.FollowUserAsync(followerId, followeeId);

            // Assert
            _mockFollowRepository.Verify(repo => repo.AddAsync(followerId, followeeId), Times.Once);
        }

        [Fact]
        public void FollowUserAsync_WithSameUserIds_ThrowsSelfFollowException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            Func<Task> act = async () => await _followCommands.FollowUserAsync(userId, userId);

            // Assert
            act.Should().ThrowAsync<SelfFollowException>();
        }

        [Fact]
        public void FollowUserAsync_WhenAlreadyFollowing_ThrowsDoubleFollowException()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            _mockFollowRepository
                .Setup(repo => repo.IsFollowingAsync(followerId, followeeId))
                .ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _followCommands.FollowUserAsync(followerId, followeeId);

            // Assert
            act.Should().ThrowAsync<DoubleFollowException>();
        }

        [Fact]
        public void FollowUserAsync_WhenFollowerBlockedFollowee_ThrowsFollowToBlockedUserException()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            _mockFollowRepository
                .Setup(repo => repo.IsFollowingAsync(followerId, followeeId))
                .ReturnsAsync(false);

            _mockRelationshipService
                .Setup(service => service.IsBlockedAsync(followerId, followeeId))
                .ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _followCommands.FollowUserAsync(followerId, followeeId);

            // Assert
            act.Should().ThrowAsync<FollowToBlockedUserException>();
        }

        [Fact]
        public void FollowUserAsync_WhenFolloweeBlockedFollower_ThrowsFollowToBlockerException()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            _mockFollowRepository
                .Setup(repo => repo.IsFollowingAsync(followerId, followeeId))
                .ReturnsAsync(false);

            _mockRelationshipService
                .Setup(service => service.IsBlockedAsync(followerId, followeeId))
                .ReturnsAsync(false);

            _mockRelationshipService
                .Setup(service => service.IsBlockedAsync(followeeId, followerId))
                .ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _followCommands.FollowUserAsync(followerId, followeeId);

            // Assert
            act.Should().ThrowAsync<FollowToBlockerException>();
        }

        [Fact]
        public async Task UnfollowUserAsync_WhenFollowingExists_CallsDeleteAsync()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            _mockFollowRepository
                .Setup(repo => repo.IsFollowingAsync(followerId, followeeId))
                .ReturnsAsync(true);

            // Act
            await _followCommands.UnfollowUserAsync(followerId, followeeId);

            // Assert
            _mockFollowRepository.Verify(repo => repo.DeleteAsync(followerId, followeeId), Times.Once);
        }

        [Fact]
        public void UnfollowUserAsync_WhenFollowingDoesNotExist_ThrowsFollowNotFoundException()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            _mockFollowRepository
                .Setup(repo => repo.IsFollowingAsync(followerId, followeeId))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _followCommands.UnfollowUserAsync(followerId, followeeId);

            // Assert
            act.Should().ThrowAsync<FollowNotFoundException>();
        }
    }
}
#endif