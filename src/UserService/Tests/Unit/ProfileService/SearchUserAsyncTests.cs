//using FluentAssertions;
//using Moq;
//using UserService.DTOs;
//using UserService.Models;
//using UserService.Services;
//using Xunit;

//namespace UserService.Tests.Unit.ProfileService
//{
//    public class SearchUserAsyncTests : UserProfileServiceTests
//    {
//        [Fact]
//        public async Task SearchUsersAsync_WithMatchingQuery_ReturnsFilteredUsers()
//        {
//            // Arrange
//            var query = "john";
//            var page = 1;
//            var pageSize = 20;

//            var allUsers = new List<UserProfile>
//            {
//                new UserProfile { Id = Guid.NewGuid(), DisplayName = "John Doe", Bio = "Developer", IsActive = true },
//                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Jane Smith", Bio = "Designer", IsActive = true },
//                new UserProfile { Id = Guid.NewGuid(), DisplayName = "Bob Johnson", Bio = "John's friend", IsActive = true }
//            };

//            var expectedFilteredUsers = allUsers
//                .Where(p => p.DisplayName.ToLower().Contains(query) || p.Bio.ToLower().Contains(query))
//                .OrderBy(p => p.DisplayName)
//                .ToList();

//            var expectedDtos = expectedFilteredUsers
//                .Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName, Bio = u.Bio })
//                .ToList();

//            ProfilesRepositoryMock
//                .Setup(x => x.GetUsersAsync())
//                .ReturnsAsync(allUsers);

//            MapperMock
//                .Setup(x => x.Map<List<UserProfileDto>>(expectedFilteredUsers))
//                .Returns(expectedDtos);

//            // Act
//            var result = await UserProfileService.SearchUsersAsync(query, page, pageSize);

//            // Assert
//            result.Should().NotBeNull();
//            result.Should().HaveCount(2);
//            result.Should().ContainSingle(u => u.DisplayName == "Bob Johnson");
//            result.Should().ContainSingle(u => u.DisplayName == "John Doe");
//            result.Should().BeInAscendingOrder(u => u.DisplayName);

//            ProfilesRepositoryMock.Verify(x => x.GetUsersAsync(), Times.Once);
//            MapperMock.Verify(x => x.Map<List<UserProfileDto>>(It.IsAny<List<UserProfile>>()), Times.Once);
//        }

//        [Fact]
//        public async Task SearchUsersAsync_WithPagination_ReturnsCorrectPage()
//        {
//            // Arrange
//            var query = "user";
//            var page = 2;
//            var pageSize = 2;

//            var allUsers = new List<UserProfile>
//            {
//                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User A", Bio = "Bio A", IsActive = true },
//                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User B", Bio = "Bio B", IsActive = true },
//                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User C", Bio = "Bio C", IsActive = true },
//                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User D", Bio = "Bio D", IsActive = true },
//                new UserProfile { Id = Guid.NewGuid(), DisplayName = "User E", Bio = "Bio E", IsActive = true }
//            };

//            var expectedFilteredUsers = allUsers
//                .OrderBy(p => p.DisplayName)
//                .Skip((page - 1) * pageSize)
//                .Take(pageSize)
//                .ToList();

//            var expectedDtos = expectedFilteredUsers
//                .Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName, Bio = u.Bio })
//                .ToList();

//            ProfilesRepositoryMock
//                .Setup(x => x.GetUsersAsync())
//                .ReturnsAsync(allUsers);

//            MapperMock
//                .Setup(x => x.Map<List<UserProfileDto>>(expectedFilteredUsers))
//                .Returns(expectedDtos);

//            // Act
//            var result = await UserProfileService.SearchUsersAsync(query, page, pageSize);

//            // Assert
//            result.Should().NotBeNull();
//            result.Should().HaveCount(2);
//            result.Should().ContainSingle(u => u.DisplayName == "User C");
//            result.Should().ContainSingle(u => u.DisplayName == "User D");

//            ProfilesRepositoryMock.Verify(x => x.GetUsersAsync(), Times.Once);
//        }

//        [Fact]
//        public async Task SearchUsersAsync_WithNoMatches_ReturnsEmptyList()
//        {
//            // Arrange
//            var query = "nonexistent";

//            var allUsers = new List<UserProfile>
//        {
//            new UserProfile { Id = Guid.NewGuid(), DisplayName = "John Doe", Bio = "Developer", IsActive = true },
//            new UserProfile { Id = Guid.NewGuid(), DisplayName = "Jane Smith", Bio = "Designer", IsActive = true }
//        };

//            var expectedDtos = new List<UserProfileDto>();

//            ProfilesRepositoryMock
//                .Setup(x => x.GetUsersAsync())
//                .ReturnsAsync(allUsers);

//            MapperMock
//                .Setup(x => x.Map<List<UserProfileDto>>(It.IsAny<List<UserProfile>>()))
//                .Returns(expectedDtos);

//            // Act
//            var result = await UserProfileService.SearchUsersAsync(query);

//            // Assert
//            result.Should().NotBeNull();
//            result.Should().BeEmpty();

//            ProfilesRepositoryMock.Verify(x => x.GetUsersAsync(), Times.Once);
//            MapperMock.Verify(x => x.Map<List<UserProfileDto>>(It.IsAny<List<UserProfile>>()), Times.Once);
//        }

//        [Fact]
//        public async Task SearchUsersAsync_ExcludesInactiveUsers()
//        {
//            // Arrange
//            var query = "test";

//            var allUsers = new List<UserProfile>
//        {
//            new UserProfile { Id = Guid.NewGuid(), DisplayName = "Active User", Bio = "test bio", IsActive = true },
//            new UserProfile { Id = Guid.NewGuid(), DisplayName = "Inactive User", Bio = "test bio", IsActive = false },
//            new UserProfile { Id = Guid.NewGuid(), DisplayName = "Another Active", Bio = "test bio", IsActive = true }
//        };

//            var expectedFilteredUsers = allUsers
//                .Where(p => p.IsActive && (p.DisplayName.Contains(query) || p.Bio.Contains(query)))
//                .OrderBy(p => p.DisplayName)
//                .ToList();

//            var expectedDtos = expectedFilteredUsers
//                .Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName, Bio = u.Bio })
//                .ToList();

//            ProfilesRepositoryMock
//                .Setup(x => x.GetUsersAsync())
//                .ReturnsAsync(allUsers);

//            MapperMock
//                .Setup(x => x.Map<List<UserProfileDto>>(expectedFilteredUsers))
//                .Returns(expectedDtos);

//            // Act
//            var result = await UserProfileService.SearchUsersAsync(query);

//            // Assert
//            result.Should().NotBeNull();
//            result.Should().HaveCount(2);
//            result.Should().NotContain(u => u.DisplayName == "Inactive User");
//            result.Should().ContainSingle(u => u.DisplayName == "Active User");
//            result.Should().ContainSingle(u => u.DisplayName == "Another Active");

//            ProfilesRepositoryMock.Verify(x => x.GetUsersAsync(), Times.Once);
//        }

//        [Fact]
//        public async Task SearchUsersAsync_WithEmptyQuery_ReturnsAllActiveUsers()
//        {
//            // Arrange
//            var query = "";

//            var allUsers = new List<UserProfile>
//        {
//            new UserProfile { Id = Guid.NewGuid(), DisplayName = "User A", Bio = "Bio A", IsActive = true },
//            new UserProfile { Id = Guid.NewGuid(), DisplayName = "User B", Bio = "Bio B", IsActive = false },
//            new UserProfile { Id = Guid.NewGuid(), DisplayName = "User C", Bio = "Bio C", IsActive = true }
//        };

//            var expectedFilteredUsers = allUsers
//                .Where(p => p.IsActive)
//                .OrderBy(p => p.DisplayName)
//                .ToList();

//            var expectedDtos = expectedFilteredUsers
//                .Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName, Bio = u.Bio })
//                .ToList();

//            ProfilesRepositoryMock
//                .Setup(x => x.GetUsersAsync())
//                .ReturnsAsync(allUsers);

//            MapperMock
//                .Setup(x => x.Map<List<UserProfileDto>>(expectedFilteredUsers))
//                .Returns(expectedDtos);

//            // Act
//            var result = await UserProfileService.SearchUsersAsync(query);

//            // Assert
//            result.Should().NotBeNull();
//            result.Should().HaveCount(2);
//            result.Should().ContainSingle(u => u.DisplayName == "User A");
//            result.Should().ContainSingle(u => u.DisplayName == "User C");
//            result.Should().NotContain(u => u.DisplayName == "User B");

//            ProfilesRepositoryMock.Verify(x => x.GetUsersAsync(), Times.Once);
//        }

//        [Fact]
//        public async Task SearchUsersAsync_WithNullQuery_ReturnsAllActiveUsers()
//        {
//            // Arrange
//            string? query = null;

//            var allUsers = new List<UserProfile>
//        {
//            new UserProfile { Id = Guid.NewGuid(), DisplayName = "User A", Bio = "Bio A", IsActive = true },
//            new UserProfile { Id = Guid.NewGuid(), DisplayName = "User B", Bio = "Bio B", IsActive = true }
//        };

//            var expectedFilteredUsers = allUsers
//                .Where(p => p.IsActive)
//                .OrderBy(p => p.DisplayName)
//                .ToList();

//            var expectedDtos = expectedFilteredUsers
//                .Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName, Bio = u.Bio })
//                .ToList();

//            ProfilesRepositoryMock
//                .Setup(x => x.GetUsersAsync())
//                .ReturnsAsync(allUsers);

//            MapperMock
//                .Setup(x => x.Map<List<UserProfileDto>>(expectedFilteredUsers))
//                .Returns(expectedDtos);

//            // Act
//            var result = await UserProfileService.SearchUsersAsync(query);

//            // Assert
//            result.Should().NotBeNull();
//            result.Should().HaveCount(2);

//            ProfilesRepositoryMock.Verify(x => x.GetUsersAsync(), Times.Once);
//        }

//        [Fact]
//        public async Task SearchUsersAsync_WithCaseSensitiveQuery_ReturnsCaseInsensitiveResults()
//        {
//            // Arrange
//            var query = "JOHN";

//            var allUsers = new List<UserProfile>
//        {
//            new UserProfile { Id = Guid.NewGuid(), DisplayName = "john doe", Bio = "Developer", IsActive = true },
//            new UserProfile { Id = Guid.NewGuid(), DisplayName = "John Smith", Bio = "Designer", IsActive = true },
//            new UserProfile { Id = Guid.NewGuid(), DisplayName = "Bob Johnson", Bio = "Friend of JOHN", IsActive = true }
//        };

//            var expectedFilteredUsers = allUsers
//                .Where(p => p.DisplayName.ToLower().Contains(query.ToLower()) || p.Bio.ToLower().Contains(query.ToLower()))
//                .OrderBy(p => p.DisplayName)
//                .ToList();

//            var expectedDtos = expectedFilteredUsers
//                .Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName, Bio = u.Bio })
//                .ToList();

//            ProfilesRepositoryMock
//                .Setup(x => x.GetUsersAsync())
//                .ReturnsAsync(allUsers);

//            MapperMock
//                .Setup(x => x.Map<List<UserProfileDto>>(expectedFilteredUsers))
//                .Returns(expectedDtos);

//            // Act
//            var result = await UserProfileService.SearchUsersAsync(query);

//            // Assert
//            result.Should().NotBeNull();
//            result.Should().HaveCount(3);

//            ProfilesRepositoryMock.Verify(x => x.GetUsersAsync(), Times.Once);
//        }

//        [Fact]
//        public async Task SearchUsersAsync_WithDefaultParameters_UsesDefaultValues()
//        {
//            // Arrange
//            var query = "test";

//            var allUsers = new List<UserProfile>
//        {
//            new UserProfile { Id = Guid.NewGuid(), DisplayName = "Test User", Bio = "Bio", IsActive = true }
//        };

//            var expectedFilteredUsers = allUsers
//                .Where(p => p.IsActive && (p.DisplayName.ToLower().Contains(query) || p.Bio.ToLower().Contains(query)))
//                .OrderBy(p => p.DisplayName)
//                .Skip(0)
//                .Take(20)
//                .ToList();

//            var expectedDtos = expectedFilteredUsers
//                .Select(u => new UserProfileDto { Id = u.Id, DisplayName = u.DisplayName, Bio = u.Bio })
//                .ToList();

//            ProfilesRepositoryMock
//                .Setup(x => x.GetUsersAsync())
//                .ReturnsAsync(allUsers);

//            MapperMock
//                .Setup(x => x.Map<List<UserProfileDto>>(expectedFilteredUsers))
//                .Returns(expectedDtos);

//            // Act
//            var result = await UserProfileService.SearchUsersAsync(query);

//            // Assert
//            result.Should().NotBeNull();
//            result.Should().HaveCount(1);

//            ProfilesRepositoryMock.Verify(x => x.GetUsersAsync(), Times.Once);
//        }
//    }
//}
