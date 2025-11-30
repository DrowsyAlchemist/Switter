using FluentAssertions;
using Moq;
using Xunit;
using UserService.Exceptions.Blocks;
using UserService.Interfaces;
using UserService.Services.Commands;
using UserService.Interfaces.Data;

namespace UserService.Tests.Unit.Commands
{
    public class BlockCommandsTests
    {
        private readonly Mock<IBlockRepository> _mockBlockRepository;
        private readonly Mock<IBlocker> _mockBlocker;
        private readonly BlockCommands _blockCommands;

        public BlockCommandsTests()
        {
            _mockBlockRepository = new Mock<IBlockRepository>();
            _mockBlocker = new Mock<IBlocker>();
            _blockCommands = new BlockCommands(_mockBlockRepository.Object, _mockBlocker.Object);
        }

        [Fact]
        public async Task BlockAsync_WithDifferentUserIds_CallsBlockUserAsync()
        {
            // Arrange
            var blockerId = Guid.NewGuid();
            var blockedId = Guid.NewGuid();

            _mockBlockRepository
                .Setup(repo => repo.IsBlockedAsync(blockerId, blockedId))
                .ReturnsAsync(false);

            // Act
            await _blockCommands.BlockAsync(blockerId, blockedId);

            // Assert
            _mockBlocker.Verify(blocker => blocker.BlockUserAsync(blockerId, blockedId), Times.Once);
            _mockBlockRepository.Verify(repo => repo.IsBlockedAsync(blockerId, blockedId), Times.Once);
        }

        [Fact]
        public void BlockAsync_WithSameUserIds_ThrowsSelfBlockException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            Func<Task> act = async () => await _blockCommands.BlockAsync(userId, userId);

            // Assert
            act.Should().ThrowAsync<SelfBlockException>();
        }

        [Fact]
        public void BlockAsync_WhenAlreadyBlocked_ThrowsDoubleBlockException()
        {
            // Arrange
            var blockerId = Guid.NewGuid();
            var blockedId = Guid.NewGuid();

            _mockBlockRepository
                .Setup(repo => repo.IsBlockedAsync(blockerId, blockedId))
                .ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _blockCommands.BlockAsync(blockerId, blockedId);

            // Assert
            act.Should().ThrowAsync<DoubleBlockException>();
        }

        [Fact]
        public async Task UnblockAsync_WhenBlockExists_CallsDeleteAsync()
        {
            // Arrange
            var blockerId = Guid.NewGuid();
            var blockedId = Guid.NewGuid();

            _mockBlockRepository
                .Setup(repo => repo.IsBlockedAsync(blockerId, blockedId))
                .ReturnsAsync(true);

            // Act
            await _blockCommands.UnblockAsync(blockerId, blockedId);

            // Assert
            _mockBlockRepository.Verify(repo => repo.DeleteAsync(blockerId, blockedId), Times.Once);
            _mockBlockRepository.Verify(repo => repo.IsBlockedAsync(blockerId, blockedId), Times.Once);
        }

        [Fact]
        public void UnblockAsync_WhenBlockDoesNotExist_ThrowsBlockNotFoundException()
        {
            // Arrange
            var blockerId = Guid.NewGuid();
            var blockedId = Guid.NewGuid();

            _mockBlockRepository
                .Setup(repo => repo.IsBlockedAsync(blockerId, blockedId))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _blockCommands.UnblockAsync(blockerId, blockedId);

            // Assert
            act.Should().ThrowAsync<BlockNotFoundException>();
        }
    }
}