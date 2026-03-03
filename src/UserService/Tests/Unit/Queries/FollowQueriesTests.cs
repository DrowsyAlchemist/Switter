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
    public class FollowQueriesTests
    {
        private readonly Mock<IFollowRepository> _mockFollowRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly FollowQueries _followQueries;

        public FollowQueriesTests()
        {
            _mockFollowRepository = new Mock<IFollowRepository>();
            _mockMapper = new Mock<IMapper>();
            _followQueries = new FollowQueries(_mockFollowRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetFollowersAsync_WithValidData_ReturnsMappedUserProfiles()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var page = 1;
            var pageSize = 20;

            var followers = new List<UserProfile>
            {
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Follower 1" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Follower 2" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Follower 3" }
            };

            var expectedDtos = new List<UserProfileDto>
            {
                new UserProfileDto { Id = followers[0].Id, DisplayName = "Follower 1" },
                new UserProfileDto { Id = followers[1].Id, DisplayName = "Follower 2" },
                new UserProfileDto { Id = followers[2].Id, DisplayName = "Follower 3" }
            };

            _mockFollowRepository
                .Setup(repo => repo.GetFollowersAsync(userId, page, pageSize))
                .ReturnsAsync(followers);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(followers))
                .Returns(expectedDtos);

            // Act
            var result = await _followQueries.GetFollowersAsync(userId, page, pageSize);

            // Assert
            result.Should().BeEquivalentTo(expectedDtos);
            _mockFollowRepository.Verify(repo => repo.GetFollowersAsync(userId, page, pageSize), Times.Once);
        }

        [Fact]
        public async Task GetFollowersAsync_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var page = 1;
            var pageSize = 20;

            var emptyUserList = new List<UserProfile>();
            var emptyDtoList = new List<UserProfileDto>();

            _mockFollowRepository
                .Setup(repo => repo.GetFollowersAsync(userId, page, pageSize))
                .ReturnsAsync(emptyUserList);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(emptyUserList))
                .Returns(emptyDtoList);

            // Act
            var result = await _followQueries.GetFollowersAsync(userId, page, pageSize);

            // Assert
            result.Should().BeEmpty();
            result.Should().BeEquivalentTo(emptyDtoList);
        }

        [Fact]
        public async Task GetFollowingAsync_WithValidData_ReturnsMappedUserProfiles()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var page = 1;
            var pageSize = 20;

            var followings = new List<UserProfile>
            {
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Following 1" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Following 2" },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Following 3" }
            };

            var expectedDtos = new List<UserProfileDto>
            {
                new UserProfileDto { Id = followings[0].Id, DisplayName = "Following 1" },
                new UserProfileDto { Id = followings[1].Id, DisplayName = "Following 2" },
                new UserProfileDto { Id = followings[2].Id, DisplayName = "Following 3" }
            };

            _mockFollowRepository
                .Setup(repo => repo.GetFollowingsAsync(userId, page, pageSize))
                .ReturnsAsync(followings);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(followings))
                .Returns(expectedDtos);

            // Act
            var result = await _followQueries.GetFollowingsAsync(userId, page, pageSize);

            // Assert
            result.Should().BeEquivalentTo(expectedDtos);
            _mockFollowRepository.Verify(repo => repo.GetFollowingsAsync(userId, page, pageSize), Times.Once);
        }

        [Fact]
        public async Task GetFollowingAsync_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var page = 1;
            var pageSize = 20;

            var emptyUserList = new List<UserProfile>();
            var emptyDtoList = new List<UserProfileDto>();

            _mockFollowRepository
                .Setup(repo => repo.GetFollowingsAsync(userId, page, pageSize))
                .ReturnsAsync(emptyUserList);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(emptyUserList))
                .Returns(emptyDtoList);

            // Act
            var result = await _followQueries.GetFollowingsAsync(userId, page, pageSize);

            // Assert
            result.Should().BeEmpty();
            result.Should().BeEquivalentTo(emptyDtoList);
        }

        [Fact]
        public async Task IsFollowingAsync_WhenFollowing_ReturnsTrue()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            _mockFollowRepository
                .Setup(repo => repo.IsFollowingAsync(followerId, followeeId))
                .ReturnsAsync(true);

            // Act
            var result = await _followQueries.IsFollowingAsync(followerId, followeeId);

            // Assert
            result.Should().BeTrue();
            _mockFollowRepository.Verify(repo => repo.IsFollowingAsync(followerId, followeeId), Times.Once);
        }

        [Fact]
        public async Task IsFollowingAsync_WhenNotFollowing_ReturnsFalse()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            _mockFollowRepository
                .Setup(repo => repo.IsFollowingAsync(followerId, followeeId))
                .ReturnsAsync(false);

            // Act
            var result = await _followQueries.IsFollowingAsync(followerId, followeeId);

            // Assert
            result.Should().BeFalse();
            _mockFollowRepository.Verify(repo => repo.IsFollowingAsync(followerId, followeeId), Times.Once);
        }

        [Theory]
        [InlineData(1, 5)]
        [InlineData(2, 10)]
        [InlineData(3, 15)]
        public async Task GetFollowersAsync_WithDifferentPageSizes_AppliesCorrectPagination(int page, int pageSize)
        {
            // Arrange
            var userId = Guid.NewGuid();

            var allUsers = Enumerable.Range(1, 50)
                .Select(i => new UserProfile { Id = Guid.NewGuid(), DisplayName = $"Follower {i}" })
                .ToList();

            var expectedUsers = allUsers.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var expectedDtos = expectedUsers.Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName }).ToList();

            _mockFollowRepository
                .Setup(repo => repo.GetFollowersAsync(userId, page, pageSize))
                .ReturnsAsync(expectedUsers);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(expectedUsers))
                .Returns(expectedDtos);

            // Act
            var result = await _followQueries.GetFollowersAsync(userId, page, pageSize);

            // Assert
            result.Should().HaveCount(expectedUsers.Count);
            result.Should().BeEquivalentTo(expectedDtos);
        }
    }
}
#endif