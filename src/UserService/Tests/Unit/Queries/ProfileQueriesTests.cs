using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;
using UserService.DTOs;
using UserService.Exceptions.Profiles;
using UserService.Interfaces;
using UserService.Interfaces.Data;
using UserService.Services.Queries;
using UserService.Models;

namespace UserService.Tests.Unit.Queries
{
    public class ProfileQueriesTests
    {
        private readonly Mock<IProfilesRepository> _mockProfilesRepository;
        private readonly Mock<IUserRelationshipService> _mockRelationshipService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ProfileQueries _profileQueries;

        public ProfileQueriesTests()
        {
            _mockProfilesRepository = new Mock<IProfilesRepository>();
            _mockRelationshipService = new Mock<IUserRelationshipService>();
            _mockMapper = new Mock<IMapper>();
            _profileQueries = new ProfileQueries(
                _mockProfilesRepository.Object,
                _mockRelationshipService.Object,
                _mockMapper.Object);
        }

        [Fact]
        public async Task GetProfileAsync_WithValidUser_ReturnsMappedProfile()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();

            var profile = new UserProfile
            {
                Id = userId,
                DisplayName = "Test User",
                Bio = "Test Bio",
                AvatarUrl = "https://example.com/avatar.jpg",
                IsActive = true
            };

            var expectedDto = new UserProfileDto
            {
                Id = userId,
                DisplayName = "Test User",
                Bio = "Test Bio",
                AvatarUrl = "https://example.com/avatar.jpg"
            };

            _mockProfilesRepository
                .Setup(repo => repo.GetProfileByIdAsync(userId))
                .ReturnsAsync(profile);

            _mockMapper
                .Setup(mapper => mapper.Map<UserProfileDto>(profile))
                .Returns(expectedDto);

            // Act
            var result = await _profileQueries.GetProfileAsync(userId, currentUserId);

            // Assert
            result.Should().BeEquivalentTo(expectedDto);
            _mockProfilesRepository.Verify(repo => repo.GetProfileByIdAsync(userId), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<UserProfileDto>(profile), Times.Once);
        }

        [Fact]
        public async Task GetProfileAsync_WithoutCurrentUserId_ReturnsMappedProfile()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var profile = new UserProfile
            {
                Id = userId,
                DisplayName = "Test User",
                IsActive = true
            };

            var expectedDto = new UserProfileDto
            {
                Id = userId,
                DisplayName = "Test User"
            };

            _mockProfilesRepository
                .Setup(repo => repo.GetProfileByIdAsync(userId))
                .ReturnsAsync(profile);

            _mockMapper
                .Setup(mapper => mapper.Map<UserProfileDto>(profile))
                .Returns(expectedDto);

            // Act
            var result = await _profileQueries.GetProfileAsync(userId);

            // Assert
            result.Should().BeEquivalentTo(expectedDto);
        }

        [Fact]
        public void GetProfileAsync_WhenUserNotFound_ThrowsUserNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockProfilesRepository
                .Setup(repo => repo.GetProfileByIdAsync(userId))
                .ReturnsAsync((UserProfile?)null);

            // Act
            Func<Task> act = async () => await _profileQueries.GetProfileAsync(userId);

            // Assert
            act.Should().ThrowAsync<UserNotFoundException>();
        }

        [Fact]
        public void GetProfileAsync_WhenUserDeactivated_ThrowsUserDeactivatedException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var deactivatedProfile = new UserProfile
            {
                Id = userId,
                DisplayName = "Deactivated User",
                IsActive = false
            };

            _mockProfilesRepository
                .Setup(repo => repo.GetProfileByIdAsync(userId))
                .ReturnsAsync(deactivatedProfile);

            // Act
            Func<Task> act = async () => await _profileQueries.GetProfileAsync(userId);

            // Assert
            act.Should().ThrowAsync<UserDeactivatedException>();
        }

        [Fact]
        public async Task SearchUsersAsync_WithQuery_ReturnsFilteredAndMappedResults()
        {
            // Arrange
            var query = "test";
            var page = 1;
            var pageSize = 20;

            var allUsers = new List<UserProfile>
            {
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Test User 1", Bio = "Some bio", IsActive = true },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Another User", Bio = "Test bio", IsActive = true },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 3", Bio = "Another bio", IsActive = true },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Test User 2", Bio = "Test bio", IsActive = true }
            };

            var expectedFilteredUsers = allUsers
                .Where(p => p.DisplayName.ToLower().Contains("test") || p.Bio.ToLower().Contains("test"))
                .OrderBy(p => p.DisplayName)
                .ToList();

            var expectedDtos = expectedFilteredUsers
                .Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName, Bio = u.Bio })
                .ToList();

            _mockProfilesRepository
                .Setup(repo => repo.GetProfilesAsync())
                .ReturnsAsync(allUsers);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(expectedFilteredUsers))
                .Returns(expectedDtos);

            // Act
            var result = await _profileQueries.SearchUsersAsync(query, page, pageSize);

            // Assert
            result.Should().BeEquivalentTo(expectedDtos);
            result.Should().HaveCount(3); // All users with "test" in display name or bio
            _mockProfilesRepository.Verify(repo => repo.GetProfilesAsync(), Times.Once);
        }

        [Fact]
        public async Task SearchUsersAsync_WithNullQuery_ConvertsToEmptyString()
        {
            // Arrange
            string? query = null;
            var page = 1;
            var pageSize = 20;

            var allUsers = new List<UserProfile>
            {
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 1", Bio = "Bio 1", IsActive = true },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 2", Bio = "Bio 2", IsActive = true }
            };

            var expectedDtos = allUsers
                .Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName, Bio = u.Bio })
                .ToList();

            _mockProfilesRepository
                .Setup(repo => repo.GetProfilesAsync())
                .ReturnsAsync(allUsers);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(allUsers))
                .Returns(expectedDtos);

            // Act
            var result = await _profileQueries.SearchUsersAsync(query, page, pageSize);

            // Assert
            result.Should().BeEquivalentTo(expectedDtos);
            result.Should().HaveCount(2); // All active users since query is empty
        }

        [Fact]
        public async Task SearchUsersAsync_WithEmptyQuery_ReturnsAllActiveUsers()
        {
            // Arrange
            var query = "";
            var page = 1;
            var pageSize = 20;

            var allUsers = new List<UserProfile>
            {
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 1", Bio = "Bio 1", IsActive = true },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 2", Bio = "Bio 2", IsActive = false }, // Inactive
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 3", Bio = "Bio 3", IsActive = true }
            };

            var expectedActiveUsers = allUsers.Where(p => p.IsActive).ToList();
            var expectedDtos = expectedActiveUsers
                .Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName, Bio = u.Bio })
                .ToList();

            _mockProfilesRepository
                .Setup(repo => repo.GetProfilesAsync())
                .ReturnsAsync(allUsers);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(expectedActiveUsers))
                .Returns(expectedDtos);

            // Act
            var result = await _profileQueries.SearchUsersAsync(query, page, pageSize);

            // Assert
            result.Should().BeEquivalentTo(expectedDtos);
            result.Should().HaveCount(2); // Only active users
        }

        [Fact]
        public async Task SearchUsersAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var query = "user";
            var page = 2;
            var pageSize = 2;

            var allUsers = new List<UserProfile>
            {
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 1", Bio = "Bio", IsActive = true },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 2", Bio = "Bio", IsActive = true },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 3", Bio = "Bio", IsActive = true },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 4", Bio = "Bio", IsActive = true },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 5", Bio = "Bio", IsActive = true }
            };

            var expectedPagedUsers = allUsers
                .OrderBy(p => p.DisplayName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var expectedDtos = expectedPagedUsers
                .Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName })
                .ToList();

            _mockProfilesRepository
                .Setup(repo => repo.GetProfilesAsync())
                .ReturnsAsync(allUsers);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(expectedPagedUsers))
                .Returns(expectedDtos);

            // Act
            var result = await _profileQueries.SearchUsersAsync(query, page, pageSize);

            // Assert
            result.Should().HaveCount(pageSize);
            result.Should().BeEquivalentTo(expectedDtos);
        }

        [Fact]
        public async Task SearchUsersAsync_WithCaseInsensitiveQuery_ReturnsMatchingResults()
        {
            // Arrange
            var query = "TEST";
            var page = 1;
            var pageSize = 20;

            var allUsers = new List<UserProfile>
            {
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "test user", Bio = "Bio", IsActive = true },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Test User", Bio = "Bio", IsActive = true },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "TEST USER", Bio = "Bio", IsActive = true },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Another", Bio = "test bio", IsActive = true }
            };

            var expectedFilteredUsers = allUsers
                .Where(p => p.DisplayName.ToLower().Contains("test") || p.Bio.ToLower().Contains("test"))
                .OrderBy(p => p.DisplayName)
                .ToList();

            var expectedDtos = expectedFilteredUsers
                .Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName })
                .ToList();

            _mockProfilesRepository
                .Setup(repo => repo.GetProfilesAsync())
                .ReturnsAsync(allUsers);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(expectedFilteredUsers))
                .Returns(expectedDtos);

            // Act
            var result = await _profileQueries.SearchUsersAsync(query, page, pageSize);

            // Assert
            result.Should().HaveCount(4); // All users should match due to case insensitivity
        }

        [Fact]
        public async Task SearchUsersAsync_WithNoMatches_ReturnsEmptyList()
        {
            // Arrange
            var query = "nonexistent";
            var page = 1;
            var pageSize = 20;

            var allUsers = new List<UserProfile>
            {
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 1", Bio = "Bio 1", IsActive = true },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 2", Bio = "Bio 2", IsActive = true }
            };

            var emptyList = new List<UserProfile>();
            var emptyDtoList = new List<UserProfileDto>();

            _mockProfilesRepository
                .Setup(repo => repo.GetProfilesAsync())
                .ReturnsAsync(allUsers);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(emptyList))
                .Returns(emptyDtoList);

            // Act
            var result = await _profileQueries.SearchUsersAsync(query, page, pageSize);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchUsersAsync_WithDefaultParameters_UsesDefaultValues()
        {
            // Arrange
            var query = "test";

            var allUsers = new List<UserProfile>
            {
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Test User", Bio = "Bio", IsActive = true }
            };

            var expectedDtos = allUsers
                .Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName })
                .ToList();

            _mockProfilesRepository
                .Setup(repo => repo.GetProfilesAsync())
                .ReturnsAsync(allUsers);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(allUsers))
                .Returns(expectedDtos);

            // Act
            var result = await _profileQueries.SearchUsersAsync(query); // Using default page and pageSize

            // Assert
            result.Should().BeEquivalentTo(expectedDtos);
        }

        [Fact]
        public async Task SearchUsersAsync_FiltersOutInactiveUsers()
        {
            // Arrange
            var query = "user";
            var page = 1;
            var pageSize = 20;

            var allUsers = new List<UserProfile>
            {
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Active User 1", Bio = "Bio", IsActive = true },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Inactive User", Bio = "Bio", IsActive = false },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Active User 2", Bio = "Bio", IsActive = true }
            };

            var expectedActiveUsers = allUsers.Where(p => p.IsActive).ToList();
            var expectedDtos = expectedActiveUsers
                .Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName })
                .ToList();

            _mockProfilesRepository
                .Setup(repo => repo.GetProfilesAsync())
                .ReturnsAsync(allUsers);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(expectedActiveUsers))
                .Returns(expectedDtos);

            // Act
            var result = await _profileQueries.SearchUsersAsync(query, page, pageSize);

            // Assert
            result.Should().HaveCount(2); // Only active users
            result.Should().NotContain(dto => dto.DisplayName == "Inactive User");
        }

        [Theory]
        [InlineData(1, 5)]
        [InlineData(2, 10)]
        [InlineData(3, 15)]
        public async Task SearchUsersAsync_WithDifferentPageSizes_AppliesCorrectPagination(int page, int pageSize)
        {
            // Arrange
            var query = "user";

            var allUsers = Enumerable.Range(1, 50)
                .Select(i => new UserProfile
                {
                    Id = Guid.NewGuid(),
                    DisplayName = $"User {i}",
                    Bio = $"Bio {i}",
                    IsActive = true
                })
                .ToList();

            var expectedUsers = allUsers
                .Where(p => p.DisplayName.ToLower().Contains("user") || p.Bio.ToLower().Contains("user"))
                .OrderBy(p => p.DisplayName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var expectedDtos = expectedUsers
                .Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName })
                .ToList();

            _mockProfilesRepository
                .Setup(repo => repo.GetProfilesAsync())
                .ReturnsAsync(allUsers);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(expectedUsers))
                .Returns(expectedDtos);

            // Act
            var result = await _profileQueries.SearchUsersAsync(query, page, pageSize);

            // Assert
            result.Should().HaveCount(expectedUsers.Count);
            result.Should().BeEquivalentTo(expectedDtos);
        }

        [Fact]
        public async Task SearchUsersAsync_WithPageBeyondData_ReturnsEmptyList()
        {
            // Arrange
            var query = "user";
            var page = 10; // Page beyond available data
            var pageSize = 10;

            var allUsers = new List<UserProfile>
            {
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 1", Bio = "Bio", IsActive = true },
                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User 2", Bio = "Bio", IsActive = true }
            };

            var emptyDtoList = new List<UserProfileDto>();

            _mockProfilesRepository
                .Setup(repo => repo.GetProfilesAsync())
                .ReturnsAsync(allUsers);

            _mockMapper
                .Setup(mapper => mapper.Map<List<UserProfileDto>>(It.IsAny<List<UserProfile>>()))
                .Returns((List<UserProfile> source) =>
                    source.Any() ? source.Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName }).ToList()
                    : emptyDtoList);

            // Act
            var result = await _profileQueries.SearchUsersAsync(query, page, pageSize);

            // Assert
            result.Should().BeEmpty();
        }
    }
}