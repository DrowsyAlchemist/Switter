using AutoMapper;
using FluentAssertions;
using Moq;
using UserService.DTOs;
using UserService.Exceptions.Blocks;
using UserService.Interfaces.Data;
using UserService.Models;
using UserService.Services;
using Xunit;

namespace UserService.Tests.Unit
{
    public class BlockServiceTests
    {
        private readonly Mock<IBlockRepository> _blockRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly BlockService _blockService;

        public BlockServiceTests()
        {
            _blockRepositoryMock = new Mock<IBlockRepository>();
            _mapperMock = new Mock<IMapper>();

            //_blockService = new BlockService(
            //    _blockRepositoryMock.Object,
            //    _mapperMock.Object
            //);
        }

        [Fact]
        public async Task BlockAsync_WithValidUsers_CompletesSuccessfully()
        {
            // Arrange
            var blockerId = Guid.NewGuid();
            var blockedId = Guid.NewGuid();

            _blockRepositoryMock
                .Setup(x => x.IsBlockedAsync(blockerId, blockedId))
                .ReturnsAsync(false);

            _blockRepositoryMock
                .Setup(x => x.AddAsync(blockerId, blockedId))
                .ReturnsAsync(It.IsAny<Block>());

            // Act
            await _blockService.BlockAsync(blockerId, blockedId);

            // Assert
            _blockRepositoryMock.Verify(x => x.IsBlockedAsync(blockerId, blockedId), Times.Once);
            _blockRepositoryMock.Verify(x => x.AddAsync(blockerId, blockedId), Times.Once);
        }

        [Fact]
        public async Task BlockAsync_WhenSelfBlock_ThrowsSelfBlockException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<SelfBlockException>(() =>
                _blockService.BlockAsync(userId, userId));

            _blockRepositoryMock.Verify(x => x.IsBlockedAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _blockRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task BlockAsync_WhenAlreadyBlocked_ThrowsDoubleBlockException()
        {
            // Arrange
            var blockerId = Guid.NewGuid();
            var blockedId = Guid.NewGuid();

            _blockRepositoryMock
                .Setup(x => x.IsBlockedAsync(blockerId, blockedId))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<DoubleBlockException>(() =>
                _blockService.BlockAsync(blockerId, blockedId));

            _blockRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UnblockAsync_WithValidUsers_CompletesSuccessfully()
        {
            // Arrange
            var blockerId = Guid.NewGuid();
            var blockedId = Guid.NewGuid();

            _blockRepositoryMock
                .Setup(x => x.IsBlockedAsync(blockerId, blockedId))
                .ReturnsAsync(true);

            _blockRepositoryMock
                .Setup(x => x.DeleteAsync(blockerId, blockedId))
                .Returns(Task.CompletedTask);

            // Act
            await _blockService.UnblockAsync(blockerId, blockedId);

            // Assert
            _blockRepositoryMock.Verify(x => x.IsBlockedAsync(blockerId, blockedId), Times.Once);
            _blockRepositoryMock.Verify(x => x.DeleteAsync(blockerId, blockedId), Times.Once);
        }

        [Fact]
        public async Task UnblockAsync_WhenNotBlocked_ThrowsFollowBlockNotFoundException()
        {
            // Arrange
            var blockerId = Guid.NewGuid();
            var blockedId = Guid.NewGuid();

            _blockRepositoryMock
                .Setup(x => x.IsBlockedAsync(blockerId, blockedId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<BlockNotFoundException>(() =>
                _blockService.UnblockAsync(blockerId, blockedId));

            _blockRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetBlockedAsync_WithValidUserId_ReturnsPaginatedBlockedUsers()
        {
            // Arrange
            var blockerId = Guid.NewGuid();
            var page = 2;
            var pageSize = 10;

            var allBlockedUsers = new List<UserProfile>
        {
            new UserProfile { Id = Guid.NewGuid(), DisplayName = "Blocked1" },
            new UserProfile { Id = Guid.NewGuid(), DisplayName = "Blocked2" },
            new UserProfile { Id = Guid.NewGuid(), DisplayName = "Blocked3" },
            new UserProfile { Id = Guid.NewGuid(), DisplayName = "Blocked4" },
            new UserProfile { Id = Guid.NewGuid(), DisplayName = "Blocked5" }
        };

            var expectedPaginatedBlockedUsers = allBlockedUsers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var expectedDtos = expectedPaginatedBlockedUsers
                .Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName })
                .ToList();

            _blockRepositoryMock
                .Setup(x => x.GetBlockedAsync(blockerId))
                .ReturnsAsync(allBlockedUsers);

            _mapperMock
                .Setup(x => x.Map<List<UserProfileDto>>(expectedPaginatedBlockedUsers))
                .Returns(expectedDtos);

            // Act
            var result = await _blockService.GetBlockedAsync(blockerId, page, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedPaginatedBlockedUsers.Count);
            result.Should().BeEquivalentTo(expectedDtos);

            _blockRepositoryMock.Verify(x => x.GetBlockedAsync(blockerId), Times.Once);
            _mapperMock.Verify(x => x.Map<List<UserProfileDto>>(expectedPaginatedBlockedUsers), Times.Once);
        }

        [Fact]
        public async Task GetBlockedAsync_WithEmptyResults_ReturnsEmptyList()
        {
            // Arrange
            var blockerId = Guid.NewGuid();

            var emptyBlockedUsers = new List<UserProfile>();
            var expectedDtos = new List<UserProfileDto>();

            _blockRepositoryMock
                .Setup(x => x.GetBlockedAsync(blockerId))
                .ReturnsAsync(emptyBlockedUsers);

            _mapperMock
                .Setup(x => x.Map<List<UserProfileDto>>(emptyBlockedUsers))
                .Returns(expectedDtos);

            // Act
            var result = await _blockService.GetBlockedAsync(blockerId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _blockRepositoryMock.Verify(x => x.GetBlockedAsync(blockerId), Times.Once);
            _mapperMock.Verify(x => x.Map<List<UserProfileDto>>(emptyBlockedUsers), Times.Once);
        }

        [Fact]
        public async Task IsBlocked_WhenBlocked_ReturnsTrue()
        {
            // Arrange
            var blockerId = Guid.NewGuid();
            var blockedId = Guid.NewGuid();

            _blockRepositoryMock
                .Setup(x => x.IsBlockedAsync(blockerId, blockedId))
                .ReturnsAsync(true);

            // Act
            var result = await _blockService.IsBlockedAsync(blockerId, blockedId);

            // Assert
            result.Should().BeTrue();
            _blockRepositoryMock.Verify(x => x.IsBlockedAsync(blockerId, blockedId), Times.Once);
        }

        [Fact]
        public async Task IsBlocked_WhenNotBlocked_ReturnsFalse()
        {
            // Arrange
            var blockerId = Guid.NewGuid();
            var blockedId = Guid.NewGuid();

            _blockRepositoryMock
                .Setup(x => x.IsBlockedAsync(blockerId, blockedId))
                .ReturnsAsync(false);

            // Act
            var result = await _blockService.IsBlockedAsync(blockerId, blockedId);

            // Assert
            result.Should().BeFalse();
            _blockRepositoryMock.Verify(x => x.IsBlockedAsync(blockerId, blockedId), Times.Once);
        }

        [Fact]
        public async Task IsBlocked_WithSameUser_ReturnsFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _blockRepositoryMock
                .Setup(x => x.IsBlockedAsync(userId, userId))
                .ReturnsAsync(false);

            // Act
            var result = await _blockService.IsBlockedAsync(userId, userId);

            // Assert
            result.Should().BeFalse();
            _blockRepositoryMock.Verify(x => x.IsBlockedAsync(userId, userId), Times.Once);
        }

        [Fact]
        public async Task GetBlockedAsync_WithLargePageNumber_ReturnsEmptyList()
        {
            // Arrange
            var blockerId = Guid.NewGuid();
            var page = 100;
            var pageSize = 20;

            var allBlockedUsers = new List<UserProfile>
        {
            new UserProfile { Id = Guid.NewGuid(), DisplayName = "Blocked1" },
            new UserProfile { Id = Guid.NewGuid(), DisplayName = "Blocked2" }
        };

            var expectedPaginatedBlockedUsers = allBlockedUsers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var expectedDtos = new List<UserProfileDto>();

            _blockRepositoryMock
                .Setup(x => x.GetBlockedAsync(blockerId))
                .ReturnsAsync(allBlockedUsers);

            _mapperMock
                .Setup(x => x.Map<List<UserProfileDto>>(expectedPaginatedBlockedUsers))
                .Returns(expectedDtos);

            // Act
            var result = await _blockService.GetBlockedAsync(blockerId, page, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _blockRepositoryMock.Verify(x => x.GetBlockedAsync(blockerId), Times.Once);
            _mapperMock.Verify(x => x.Map<List<UserProfileDto>>(expectedPaginatedBlockedUsers), Times.Once);
        }
    }
}
