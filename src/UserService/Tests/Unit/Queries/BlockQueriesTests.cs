#if DEBUG
using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;
using UserService.DTOs;
using UserService.Interfaces.Data;
using UserService.Services.Queries;
using UserService.Models;

namespace UserService.Tests.Unit.Queries
{
    public class BlockQueriesTests
    {
        private readonly Mock<IBlockRepository> _mockBlockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly BlockQueries _blockQueries;

        public BlockQueriesTests()
        {
            _mockBlockRepository = new Mock<IBlockRepository>();
            _mockMapper = new Mock<IMapper>();
            _blockQueries = new BlockQueries(_mockBlockRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetBlockedAsync_WithValidData_ReturnsMappedUserProfiles()
        {
            // Arrange
            var blockerId = Guid.NewGuid();
            var page = 1;
            var pageSize = 20;

            var blockedUsers = new List<UserProfile>
            {
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 1" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 2" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 3" }
            };

            var expectedDtos = new List<UserProfileDto>
            {
                new UserProfileDto { Id = blockedUsers[0].Id, DisplayName = "User 1" },
                new UserProfileDto { Id = blockedUsers[1].Id, DisplayName = "User 2" },
                new UserProfileDto { Id = blockedUsers[2].Id, DisplayName = "User 3" }
            };

            _mockBlockRepository
                .Setup(repo => repo.GetBlockedAsync(blockerId, page, pageSize))
                .ReturnsAsync(blockedUsers);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(blockedUsers))
                .Returns(expectedDtos);

            // Act
            var result = await _blockQueries.GetBlockedAsync(blockerId, page, pageSize);

            // Assert
            result.Should().BeEquivalentTo(expectedDtos);
            _mockBlockRepository.Verify(repo => repo.GetBlockedAsync(blockerId, page, pageSize), Times.Once);
        }

        [Fact]
        public async Task GetBlockedAsync_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var blockerId = Guid.NewGuid();
            var page = 1;
            var pageSize = 20;

            var emptyUserList = new List<UserProfile>();
            var emptyDtoList = new List<UserProfileDto>();

            _mockBlockRepository
                .Setup(repo => repo.GetBlockedAsync(blockerId, page, pageSize))
                .ReturnsAsync(emptyUserList);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(emptyUserList))
                .Returns(emptyDtoList);

            // Act
            var result = await _blockQueries.GetBlockedAsync(blockerId, page, pageSize);

            // Assert
            result.Should().BeEmpty();
            result.Should().BeEquivalentTo(emptyDtoList);
        }

        [Fact]
        public async Task IsBlockedAsync_WhenBlocked_ReturnsTrue()
        {
            // Arrange
            var blockerId = Guid.NewGuid();
            var blockedId = Guid.NewGuid();

            _mockBlockRepository
                .Setup(repo => repo.IsBlockedAsync(blockerId, blockedId))
                .ReturnsAsync(true);

            // Act
            var result = await _blockQueries.IsBlockedAsync(blockerId, blockedId);

            // Assert
            result.Should().BeTrue();
            _mockBlockRepository.Verify(repo => repo.IsBlockedAsync(blockerId, blockedId), Times.Once);
        }

        [Fact]
        public async Task IsBlockedAsync_WhenNotBlocked_ReturnsFalse()
        {
            // Arrange
            var blockerId = Guid.NewGuid();
            var blockedId = Guid.NewGuid();

            _mockBlockRepository
                .Setup(repo => repo.IsBlockedAsync(blockerId, blockedId))
                .ReturnsAsync(false);

            // Act
            var result = await _blockQueries.IsBlockedAsync(blockerId, blockedId);

            // Assert
            result.Should().BeFalse();
            _mockBlockRepository.Verify(repo => repo.IsBlockedAsync(blockerId, blockedId), Times.Once);
        }

        [Fact]
        public async Task GetBlockedAsync_VerifiesMapperWasCalledWithCorrectData()
        {
            // Arrange
            var blockerId = Guid.NewGuid();
            var page = 1;
            var pageSize = 10;

            var blockedUsers = new List<UserProfile>
            {
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 1" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 2" }
            };

            var expectedDtos = new List<UserProfileDto>
            {
                new UserProfileDto { Id = blockedUsers[0].Id, DisplayName = "User 1" },
                new UserProfileDto { Id = blockedUsers[1].Id, DisplayName = "User 2" }
            };

            _mockBlockRepository
                .Setup(repo => repo.GetBlockedAsync(blockerId, page, pageSize))
                .ReturnsAsync(blockedUsers);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(blockedUsers))
                .Returns(expectedDtos);

            // Act
            await _blockQueries.GetBlockedAsync(blockerId, page, pageSize);

            // Assert
            _mockMapper.Verify(mapper => mapper.Map<List<UserProfileDto>>(blockedUsers), Times.Once);
        }
    }
}
#endif