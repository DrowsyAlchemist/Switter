//using FluentAssertions;
//using Moq;
//using System.Text.Json;
//using UserService.DTOs;
//using UserService.Exceptions.Profiles;
//using UserService.Models;
//using Xunit;

//namespace UserService.Tests.Unit.ProfileService
//{
//    public class GetProfileAsyncTests : UserProfileServiceTests
//    {
//        [Fact]
//        public async Task GetProfileAsync_WithCachedData_ReturnsCachedProfile()
//        {
//            // Arrange
//            var userId = Guid.NewGuid();
//            var currentUserId = Guid.NewGuid();
//            var expectedProfile = new UserProfileDto
//            {
//                Id = userId,
//                DisplayName = "Test User",
//                IsFollowed = false
//            };

//            var cachedJson = JsonSerializer.Serialize(expectedProfile);

//            RedisServiceMock
//                .Setup(x => x.GetAsync(It.IsAny<string>()))
//                .ReturnsAsync(cachedJson);

//            FollowCheckerMock
//                .Setup(x => x.IsFollowingAsync(currentUserId, userId))
//                .ReturnsAsync(false);

//            // Act
//            var result = await UserProfileService.GetProfileAsync(userId, currentUserId);

//            // Assert
//            result.Should().NotBeNull();
//            result.Id.Should().Be(expectedProfile.Id);
//            result.DisplayName.Should().Be(expectedProfile.DisplayName);
//            result.IsFollowed.Should().BeFalse();

//            RedisServiceMock.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Once);
//            ProfilesRepositoryMock.Verify(x => x.GetProfileAsync(It.IsAny<Guid>()), Times.Never);
//            FollowCheckerMock.Verify(x => x.IsFollowingAsync(currentUserId, userId), Times.Once);
//        }

//        [Fact]
//        public async Task GetProfileAsync_WithCachedDataAndNoCurrentUser_ReturnsCachedProfileWithoutFollowCheck()
//        {
//            // Arrange
//            var userId = Guid.NewGuid();
//            var expectedProfile = new UserProfileDto
//            {
//                Id = userId,
//                DisplayName = "Test User"
//            };

//            var cachedJson = JsonSerializer.Serialize(expectedProfile);

//            RedisServiceMock
//                .Setup(x => x.GetAsync(It.IsAny<string>()))
//                .ReturnsAsync(cachedJson);

//            // Act
//            var result = await UserProfileService.GetProfileAsync(userId);

//            // Assert
//            result.Should().NotBeNull();
//            result.Id.Should().Be(expectedProfile.Id);

//            FollowCheckerMock.Verify(x => x.IsFollowingAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
//        }

//        [Fact]
//        public async Task GetProfileAsync_WithoutCachedData_ReturnsProfileFromRepository()
//        {
//            // Arrange
//            var userId = Guid.NewGuid();
//            var currentUserId = Guid.NewGuid();
//            var userProfile = new UserProfile { Id = userId, DisplayName = "Test User", IsActive = true };
//            var expectedProfileDto = new UserProfileDto { Id = userId, DisplayName = "Test User", IsFollowed = true };

//            RedisServiceMock
//                .Setup(x => x.GetAsync(It.IsAny<string>()))
//                .ReturnsAsync((string?)null);

//            ProfilesRepositoryMock
//                .Setup(x => x.GetProfileAsync(userId))
//                .ReturnsAsync(userProfile);

//            MapperMock
//                .Setup(x => x.Map<UserProfileDto>(userProfile))
//                .Returns(expectedProfileDto);

//            FollowCheckerMock
//                .Setup(x => x.IsFollowingAsync(currentUserId, userId))
//                .ReturnsAsync(true);

//            RedisServiceMock
//                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
//                .Returns(Task.CompletedTask);

//            // Act
//            var result = await UserProfileService.GetProfileAsync(userId, currentUserId);

//            // Assert
//            result.Should().NotBeNull();
//            result.Id.Should().Be(expectedProfileDto.Id);
//            result.IsFollowed.Should().BeTrue();

//            ProfilesRepositoryMock.Verify(x => x.GetProfileAsync(userId), Times.Once);
//            MapperMock.Verify(x => x.Map<UserProfileDto>(userProfile), Times.Once);
//            RedisServiceMock.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Once);
//        }

//        [Fact]
//        public async Task GetProfileAsync_WithoutCachedDataAndUserNotFound_ThrowsUserNotFoundException()
//        {
//            // Arrange
//            var userId = Guid.NewGuid();

//            RedisServiceMock
//                .Setup(x => x.GetAsync(It.IsAny<string>()))
//                .ReturnsAsync((string?)null);

//            ProfilesRepositoryMock
//                .Setup(x => x.GetProfileAsync(userId))
//                .ReturnsAsync((UserProfile?)null);

//            // Act & Assert
//            await Assert.ThrowsAsync<UserNotFoundException>(() =>
//                UserProfileService.GetProfileAsync(userId));
//        }

//        [Fact]
//        public async Task GetProfileAsync_WithoutCachedDataAndUserDeactivated_ThrowsUserDeactivatedException()
//        {
//            // Arrange
//            var userId = Guid.NewGuid();
//            var userProfile = new UserProfile { Id = userId, IsActive = false };

//            RedisServiceMock
//                .Setup(x => x.GetAsync(It.IsAny<string>()))
//                .ReturnsAsync((string?)null);

//            ProfilesRepositoryMock
//                .Setup(x => x.GetProfileAsync(userId))
//                .ReturnsAsync(userProfile);

//            // Act & Assert
//            await Assert.ThrowsAsync<UserDeactivatedException>(() =>
//                UserProfileService.GetProfileAsync(userId));
//        }

//        [Fact]
//        public async Task GetProfileAsync_WithInvalidCachedData_FallsBackToRepository()
//        {
//            // Arrange
//            var userId = Guid.NewGuid();
//            var userProfile = new UserProfile { Id = userId, DisplayName = "Test User", IsActive = true };
//            var expectedProfileDto = new UserProfileDto { Id = userId, DisplayName = "Test User" };
//            var invalidCachedJson = "invalid json";

//            RedisServiceMock
//                .Setup(x => x.GetAsync(It.IsAny<string>()))
//                .ReturnsAsync(invalidCachedJson);

//            ProfilesRepositoryMock
//                .Setup(x => x.GetProfileAsync(userId))
//                .ReturnsAsync(userProfile);

//            MapperMock
//                .Setup(x => x.Map<UserProfileDto>(userProfile))
//                .Returns(expectedProfileDto);

//            RedisServiceMock
//                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
//                .Returns(Task.CompletedTask);

//            // Act
//            var result = await UserProfileService.GetProfileAsync(userId);

//            // Assert
//            result.Should().NotBeNull();
//            result.Id.Should().Be(expectedProfileDto.Id);

//            ProfilesRepositoryMock.Verify(x => x.GetProfileAsync(userId), Times.Once);
//            RedisServiceMock.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Once);
//        }

//        [Fact]
//        public async Task GetProfileAsync_WithoutCachedData_SavesToCache()
//        {
//            // Arrange
//            var userId = Guid.NewGuid();
//            var userProfile = new UserProfile { Id = userId, DisplayName = "Test User", IsActive = true };
//            var expectedProfileDto = new UserProfileDto { Id = userId, DisplayName = "Test User" };

//            RedisServiceMock
//                .Setup(x => x.GetAsync(It.IsAny<string>()))
//                .ReturnsAsync((string?)null);

//            ProfilesRepositoryMock
//                .Setup(x => x.GetProfileAsync(userId))
//                .ReturnsAsync(userProfile);

//            MapperMock
//                .Setup(x => x.Map<UserProfileDto>(userProfile))
//                .Returns(expectedProfileDto);

//            string savedCacheKey = null!;
//            string savedCacheValue = null!;
//            TimeSpan? savedExpiration = null;

//            RedisServiceMock
//                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
//                .Callback<string, string, TimeSpan?>((key, value, expiration) =>
//                {
//                    savedCacheKey = key;
//                    savedCacheValue = value;
//                    savedExpiration = expiration;
//                })
//                .Returns(Task.CompletedTask);

//            // Act
//            var result = await UserProfileService.GetProfileAsync(userId);

//            // Assert
//            result.Should().NotBeNull();
//            savedCacheKey.Should().NotBeNull();
//            savedCacheValue.Should().NotBeNull();
//            savedExpiration.Should().NotBeNull();

//            var cachedDto = JsonSerializer.Deserialize<UserProfileDto>(savedCacheValue);
//            cachedDto.Should().NotBeNull();
//            cachedDto.Id.Should().Be(expectedProfileDto.Id);
//        }
//    }
//}
