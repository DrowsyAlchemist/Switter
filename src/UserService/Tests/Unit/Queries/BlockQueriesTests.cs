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
                .Setup(repo => repo.GetBlockedAsync(blockerId))
                .ReturnsAsync(blockedUsers);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(blockedUsers))
                .Returns(expectedDtos);

            // Act
            var result = await _blockQueries.GetBlockedAsync(blockerId, page, pageSize);

            // Assert
            result.Should().BeEquivalentTo(expectedDtos);
            _mockBlockRepository.Verify(repo => repo.GetBlockedAsync(blockerId), Times.Once);
        }

        [Fact]
        public async Task GetBlockedAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var blockerId = Guid.NewGuid();
            var page = 2;
            var pageSize = 2;

            var allBlockedUsers = new List<UserProfile>
            {
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 1" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 2" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 3" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 4" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 5" }
            };

            var expectedPagedUsers = allBlockedUsers.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var expectedDtos = expectedPagedUsers.Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName }).ToList();

            _mockBlockRepository
                .Setup(repo => repo.GetBlockedAsync(blockerId))
                .ReturnsAsync(allBlockedUsers);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(expectedPagedUsers))
                .Returns(expectedDtos);

            // Act
            var result = await _blockQueries.GetBlockedAsync(blockerId, page, pageSize);

            // Assert
            result.Should().HaveCount(pageSize);
            result.Should().BeEquivalentTo(expectedDtos);
            result.Select(r => r.DisplayName).Should().ContainInOrder("User 3", "User 4");
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
                .Setup(repo => repo.GetBlockedAsync(blockerId))
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
        public async Task GetBlockedAsync_WithPageBeyondData_ReturnsEmptyList()
        {
            // Arrange
            var blockerId = Guid.NewGuid();
            var page = 10; // Page beyond available data
            var pageSize = 10;

            var blockedUsers = new List<UserProfile>
            {
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 1" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 2" }
            };

            var emptyDtoList = new List<UserProfileDto>();

            _mockBlockRepository
                .Setup(repo => repo.GetBlockedAsync(blockerId))
                .ReturnsAsync(blockedUsers);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(It.IsAny<List<UserProfile>>()))
                .Returns((List<UserProfile> source) =>
                    source.Any() ? source.Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName }).ToList()
                    : emptyDtoList);

            // Act
            var result = await _blockQueries.GetBlockedAsync(blockerId, page, pageSize);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetBlockedAsync_WithDefaultParameters_UsesDefaultValues()
        {
            // Arrange
            var blockerId = Guid.NewGuid();

            var blockedUsers = new List<UserProfile>
            {
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 1" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 2" }
            };

            var expectedDtos = blockedUsers.Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName }).ToList();

            _mockBlockRepository
                .Setup(repo => repo.GetBlockedAsync(blockerId))
                .ReturnsAsync(blockedUsers);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(blockedUsers))
                .Returns(expectedDtos);

            // Act
            var result = await _blockQueries.GetBlockedAsync(blockerId); // Using default page and pageSize

            // Assert
            result.Should().BeEquivalentTo(expectedDtos);
            _mockBlockRepository.Verify(repo => repo.GetBlockedAsync(blockerId), Times.Once);
        }

        [Fact]
        public async Task GetBlockedAsync_WithLargePageSize_ReturnsAllAvailableUsers()
        {
            // Arrange
            var blockerId = Guid.NewGuid();
            var page = 1;
            var pageSize = 100; // Larger than available data

            var blockedUsers = new List<UserProfile>
            {
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 1" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 2" }
            };

            var expectedDtos = blockedUsers.Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName }).ToList();

            _mockBlockRepository
                .Setup(repo => repo.GetBlockedAsync(blockerId))
                .ReturnsAsync(blockedUsers);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(blockedUsers))
                .Returns(expectedDtos);

            // Act
            var result = await _blockQueries.GetBlockedAsync(blockerId, page, pageSize);

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedDtos);
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
                .Setup(repo => repo.GetBlockedAsync(blockerId))
                .ReturnsAsync(blockedUsers);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(blockedUsers))
                .Returns(expectedDtos);

            // Act
            await _blockQueries.GetBlockedAsync(blockerId, page, pageSize);

            // Assert
            _mockMapper.Verify(mapper => mapper.Map<List<UserProfileDto>>(blockedUsers), Times.Once);
        }

        [Theory]
        [InlineData(1, 5)]
        [InlineData(2, 10)]
        [InlineData(3, 15)]
        public async Task GetBlockedAsync_WithDifferentPageSizes_AppliesCorrectPagination(int page, int pageSize)
        {
            // Arrange
            var blockerId = Guid.NewGuid();

            var allUsers = Enumerable.Range(1, 50)
                .Select(i => new UserProfile { Id = Guid.NewGuid(), DisplayName = $"User {i}" })
                .ToList();

            var expectedUsers = allUsers.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var expectedDtos = expectedUsers.Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName }).ToList();

            _mockBlockRepository
                .Setup(repo => repo.GetBlockedAsync(blockerId))
                .ReturnsAsync(allUsers);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(expectedUsers))
                .Returns(expectedDtos);

            // Act
            var result = await _blockQueries.GetBlockedAsync(blockerId, page, pageSize);

            // Assert
            result.Should().HaveCount(expectedUsers.Count);
            result.Should().BeEquivalentTo(expectedDtos);
        }
    }
}