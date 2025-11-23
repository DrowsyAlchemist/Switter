//using FluentAssertions;
//using Moq;
//using UserService.DTOs;
//using UserService.Exceptions.Profiles;
//using UserService.Models;
//using Xunit;

//namespace UserService.Tests.Unit.ProfileService
//{
//    public class UpdateProfileAsyncTests : UserProfileServiceTests
//    {
//        [Fact]
//        public async Task UpdateProfileAsync_WithValidRequest_UpdatesProfileAndClearsCache()
//        {
//            // Arrange
//            var userId = Guid.NewGuid();
//            var request = new UpdateProfileRequest
//            {
//                DisplayName = "New Display Name",
//                Bio = "New Bio",
//                AvatarUrl = "https://example.com/avatar.jpg"
//            };

//            var existingProfile = new UserProfile
//            {
//                Id = userId,
//                DisplayName = "Old Name",
//                Bio = "Old Bio",
//                AvatarUrl = "https://example.com/old.jpg",
//                IsActive = true
//            };

//            var updatedProfileDto = new UserProfileDto
//            {
//                Id = userId,
//                DisplayName = "New Display Name",
//                Bio = "New Bio",
//                AvatarUrl = "https://example.com/avatar.jpg"
//            };

//            ProfilesRepositoryMock
//                .Setup(x => x.GetProfileAsync(userId))
//                .ReturnsAsync(existingProfile);

//            ProfilesRepositoryMock
//                .Setup(x => x.UpdateProfileAsync(It.IsAny<UserProfile>()))
//                .ReturnsAsync(It.IsAny<UserProfile>());

//            RedisServiceMock
//                .Setup(x => x.RemoveAsync(It.IsAny<string>()))
//                .Returns(Task.CompletedTask);

//            MapperMock
//                .Setup(x => x.Map<UserProfileDto>(It.IsAny<UserProfile>()))
//                .Returns(updatedProfileDto);

//            // Act
//            var result = await UserProfileService.UpdateProfileAsync(userId, request);

//            // Assert
//            result.Should().NotBeNull();
//            result.DisplayName.Should().Be(request.DisplayName);
//            result.Bio.Should().Be(request.Bio);
//            result.AvatarUrl.Should().Be(request.AvatarUrl);

//            ProfilesRepositoryMock.Verify(x => x.GetProfileAsync(userId), Times.Once);
//            ProfilesRepositoryMock.Verify(x => x.UpdateProfileAsync(It.Is<UserProfile>(p =>
//                p.DisplayName == request.DisplayName &&
//                p.Bio == request.Bio &&
//                p.AvatarUrl == request.AvatarUrl &&
//                p.UpdatedAt > DateTime.UtcNow.AddMinutes(-1)
//            )), Times.Once);

//            RedisServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);
//            MapperMock.Verify(x => x.Map<UserProfileDto>(It.IsAny<UserProfile>()), Times.Once);
//        }

//        [Fact]
//        public async Task UpdateProfileAsync_WithPartialUpdate_UpdatesOnlyProvidedFields()
//        {
//            // Arrange
//            var userId = Guid.NewGuid();
//            var request = new UpdateProfileRequest
//            {
//                DisplayName = "New Display Name"
//            };

//            var existingProfile = new UserProfile
//            {
//                Id = userId,
//                DisplayName = "Old Name",
//                Bio = "Existing Bio",
//                AvatarUrl = "https://example.com/existing.jpg",
//                IsActive = true
//            };

//            var updatedProfileDto = new UserProfileDto
//            {
//                Id = userId,
//                DisplayName = "New Display Name",
//                Bio = "Existing Bio",
//                AvatarUrl = "https://example.com/existing.jpg"
//            };

//            ProfilesRepositoryMock
//                .Setup(x => x.GetProfileAsync(userId))
//                .ReturnsAsync(existingProfile);

//            ProfilesRepositoryMock
//                .Setup(x => x.UpdateProfileAsync(It.IsAny<UserProfile>()))
//                .ReturnsAsync(It.IsAny<UserProfile>());

//            RedisServiceMock
//                .Setup(x => x.RemoveAsync(It.IsAny<string>()))
//                .Returns(Task.CompletedTask);

//            MapperMock
//                .Setup(x => x.Map<UserProfileDto>(It.IsAny<UserProfile>()))
//                .Returns(updatedProfileDto);

//            // Act
//            var result = await UserProfileService.UpdateProfileAsync(userId, request);

//            // Assert
//            result.Should().NotBeNull();
//            result.DisplayName.Should().Be(request.DisplayName);
//            result.Bio.Should().Be(existingProfile.Bio);
//            result.AvatarUrl.Should().Be(existingProfile.AvatarUrl);

//            ProfilesRepositoryMock.Verify(x => x.UpdateProfileAsync(It.Is<UserProfile>(p =>
//                p.DisplayName == request.DisplayName &&
//                p.Bio == existingProfile.Bio &&
//                p.AvatarUrl == existingProfile.AvatarUrl
//            )), Times.Once);
//        }

//        [Fact]
//        public async Task UpdateProfileAsync_WithEmptyDisplayName_DoesNotUpdateDisplayName()
//        {
//            // Arrange
//            var userId = Guid.NewGuid();
//            var request = new UpdateProfileRequest
//            {
//                DisplayName = "",
//                Bio = "New Bio"
//            };

//            var existingProfile = new UserProfile
//            {
//                Id = userId,
//                DisplayName = "Original Name",
//                Bio = "Old Bio",
//                IsActive = true
//            };

//            var updatedProfileDto = new UserProfileDto
//            {
//                Id = userId,
//                DisplayName = "Original Name", // должно остаться прежним
//                Bio = "New Bio"
//            };

//            ProfilesRepositoryMock
//                .Setup(x => x.GetProfileAsync(userId))
//                .ReturnsAsync(existingProfile);

//            ProfilesRepositoryMock
//                .Setup(x => x.UpdateProfileAsync(It.IsAny<UserProfile>()))
//                .ReturnsAsync(It.IsAny<UserProfile>());

//            RedisServiceMock
//                .Setup(x => x.RemoveAsync(It.IsAny<string>()))
//                .Returns(Task.CompletedTask);

//            MapperMock
//                .Setup(x => x.Map<UserProfileDto>(It.IsAny<UserProfile>()))
//                .Returns(updatedProfileDto);

//            // Act
//            var result = await UserProfileService.UpdateProfileAsync(userId, request);

//            // Assert
//            result.Should().NotBeNull();
//            result.DisplayName.Should().Be(existingProfile.DisplayName);
//            result.Bio.Should().Be(request.Bio);

//            ProfilesRepositoryMock.Verify(x => x.UpdateProfileAsync(It.Is<UserProfile>(p =>
//                p.DisplayName == existingProfile.DisplayName &&
//                p.Bio == request.Bio
//            )), Times.Once);
//        }

//        [Fact]
//        public async Task UpdateProfileAsync_WhenUserNotFound_ThrowsUserNotFoundException()
//        {
//            // Arrange
//            var userId = Guid.NewGuid();
//            var request = new UpdateProfileRequest
//            {
//                DisplayName = "New Name"
//            };

//            ProfilesRepositoryMock
//                .Setup(x => x.GetProfileAsync(userId))
//                .ReturnsAsync((UserProfile?)null);

//            // Act & Assert
//            await Assert.ThrowsAsync<UserNotFoundException>(() =>
//                UserProfileService.UpdateProfileAsync(userId, request));

//            ProfilesRepositoryMock.Verify(x => x.UpdateProfileAsync(It.IsAny<UserProfile>()), Times.Never);
//            RedisServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Never);
//        }

//        [Fact]
//        public async Task UpdateProfileAsync_WhenUserDeactivated_ThrowsUserDeactivatedException()
//        {
//            // Arrange
//            var userId = Guid.NewGuid();
//            var request = new UpdateProfileRequest
//            {
//                DisplayName = "New Name"
//            };

//            var deactivatedProfile = new UserProfile
//            {
//                Id = userId,
//                IsActive = false
//            };

//            ProfilesRepositoryMock
//                .Setup(x => x.GetProfileAsync(userId))
//                .ReturnsAsync(deactivatedProfile);

//            // Act & Assert
//            await Assert.ThrowsAsync<UserDeactivatedException>(() =>
//                UserProfileService.UpdateProfileAsync(userId, request));

//            ProfilesRepositoryMock.Verify(x => x.UpdateProfileAsync(It.IsAny<UserProfile>()), Times.Never);
//            RedisServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Never);
//        }

//        [Fact]
//        public async Task UpdateProfileAsync_WithAllNullFields_OnlyUpdatesTimestamp()
//        {
//            // Arrange
//            var userId = Guid.NewGuid();
//            var request = new UpdateProfileRequest();

//            var existingProfile = new UserProfile
//            {
//                Id = userId,
//                DisplayName = "Original Name",
//                Bio = "Original Bio",
//                AvatarUrl = "https://example.com/original.jpg",
//                IsActive = true
//            };

//            var updatedProfileDto = new UserProfileDto
//            {
//                Id = userId,
//                DisplayName = "Original Name",
//                Bio = "Original Bio",
//                AvatarUrl = "https://example.com/original.jpg"
//            };

//            ProfilesRepositoryMock
//                .Setup(x => x.GetProfileAsync(userId))
//                .ReturnsAsync(existingProfile);

//            ProfilesRepositoryMock
//                .Setup(x => x.UpdateProfileAsync(It.IsAny<UserProfile>()))
//                .ReturnsAsync(existingProfile);

//            RedisServiceMock
//                .Setup(x => x.RemoveAsync(It.IsAny<string>()))
//                .Returns(Task.CompletedTask);

//            MapperMock
//                .Setup(x => x.Map<UserProfileDto>(It.IsAny<UserProfile>()))
//                .Returns(updatedProfileDto);

//            // Act
//            var result = await UserProfileService.UpdateProfileAsync(userId, request);

//            // Assert
//            result.Should().NotBeNull();

//            ProfilesRepositoryMock.Verify(x => x.UpdateProfileAsync(It.Is<UserProfile>(p =>
//                p.DisplayName == existingProfile.DisplayName &&
//                p.Bio == existingProfile.Bio &&
//                p.AvatarUrl == existingProfile.AvatarUrl &&
//                p.UpdatedAt > DateTime.UtcNow.AddMinutes(-1)
//            )), Times.Once);

//            RedisServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);
//        }
//    }
//}
