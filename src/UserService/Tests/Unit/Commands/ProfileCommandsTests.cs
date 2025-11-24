using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;
using UserService.DTOs;
using UserService.Exceptions.Profiles;
using UserService.Interfaces.Data;
using UserService.Services.Commands;
using UserService.Models;

namespace UserService.Tests.Unit.Commands
{
    public class ProfileCommandsTests
    {
        private readonly Mock<IProfilesRepository> _mockProfilesRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ProfileCommands _profileCommands;

        public ProfileCommandsTests()
        {
            _mockProfilesRepository = new Mock<IProfilesRepository>();
            _mockMapper = new Mock<IMapper>();
            _profileCommands = new ProfileCommands(_mockProfilesRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task UpdateProfileAsync_WithValidData_UpdatesProfileAndReturnsDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new UpdateProfileRequest
            {
                DisplayName = "New Display Name",
                Bio = "New Bio",
                AvatarUrl = "https://example.com/avatar.jpg"
            };

            var existingProfile = new UserProfile
            {
                Id = userId,
                DisplayName = "Old Display Name",
                Bio = "Old Bio",
                AvatarUrl = "https://example.com/old-avatar.jpg",
                IsActive = true,
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var updatedProfileDto = new UserProfileDto
            {
                Id = userId,
                DisplayName = "New Display Name",
                Bio = "New Bio",
                AvatarUrl = "https://example.com/avatar.jpg"
            };

            _mockProfilesRepository
                .Setup(repo => repo.GetProfileAsync(userId))
                .ReturnsAsync(existingProfile);

            _mockMapper
                .Setup(mapper => mapper.Map<UserProfileDto>(It.IsAny<UserProfile>()))
                .Returns(updatedProfileDto);

            // Act
            var result = await _profileCommands.UpdateProfileAsync(userId, request);

            // Assert
            result.Should().BeEquivalentTo(updatedProfileDto);
            existingProfile.DisplayName.Should().Be(request.DisplayName);
            existingProfile.Bio.Should().Be(request.Bio);
            existingProfile.AvatarUrl.Should().Be(request.AvatarUrl);
            existingProfile.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

            _mockProfilesRepository.Verify(repo => repo.UpdateProfileAsync(existingProfile), Times.Once);
        }

        [Fact]
        public async Task UpdateProfileAsync_WithPartialData_UpdatesOnlyProvidedFields()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new UpdateProfileRequest
            {
                DisplayName = "New Display Name"
                // Bio and AvatarUrl are null
            };

            var existingProfile = new UserProfile
            {
                Id = userId,
                DisplayName = "Old Display Name",
                Bio = "Old Bio",
                AvatarUrl = "https://example.com/old-avatar.jpg",
                IsActive = true,
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var updatedProfileDto = new UserProfileDto
            {
                Id = userId,
                DisplayName = "New Display Name",
                Bio = "Old Bio", // Should remain unchanged
                AvatarUrl = "https://example.com/old-avatar.jpg" // Should remain unchanged
            };

            _mockProfilesRepository
                .Setup(repo => repo.GetProfileAsync(userId))
                .ReturnsAsync(existingProfile);

            _mockMapper
                .Setup(mapper => mapper.Map<UserProfileDto>(It.IsAny<UserProfile>()))
                .Returns(updatedProfileDto);

            // Act
            var result = await _profileCommands.UpdateProfileAsync(userId, request);

            // Assert
            result.Should().BeEquivalentTo(updatedProfileDto);
            existingProfile.DisplayName.Should().Be(request.DisplayName);
            existingProfile.Bio.Should().Be("Old Bio"); // Unchanged
            existingProfile.AvatarUrl.Should().Be("https://example.com/old-avatar.jpg"); // Unchanged
            existingProfile.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task UpdateProfileAsync_WithEmptyDisplayName_DoesNotUpdateDisplayName()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new UpdateProfileRequest
            {
                DisplayName = "", // Empty string
                Bio = "New Bio"
            };

            var existingProfile = new UserProfile
            {
                Id = userId,
                DisplayName = "Old Display Name",
                Bio = "Old Bio",
                AvatarUrl = "https://example.com/avatar.jpg",
                IsActive = true,
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var updatedProfileDto = new UserProfileDto
            {
                Id = userId,
                DisplayName = "Old Display Name", // Should remain unchanged
                Bio = "New Bio"
            };

            _mockProfilesRepository
                .Setup(repo => repo.GetProfileAsync(userId))
                .ReturnsAsync(existingProfile);

            _mockMapper
                .Setup(mapper => mapper.Map<UserProfileDto>(It.IsAny<UserProfile>()))
                .Returns(updatedProfileDto);

            // Act
            var result = await _profileCommands.UpdateProfileAsync(userId, request);

            // Assert
            existingProfile.DisplayName.Should().Be("Old Display Name"); // Unchanged due to empty string
            existingProfile.Bio.Should().Be("New Bio"); // Updated
        }

        [Fact]
        public async Task UpdateProfileAsync_WithNullBio_DoesNotUpdateBio()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new UpdateProfileRequest
            {
                DisplayName = "New Display Name",
                Bio = null // Explicitly setting to null
            };

            var existingProfile = new UserProfile
            {
                Id = userId,
                DisplayName = "Old Display Name",
                Bio = "Old Bio",
                AvatarUrl = "https://example.com/avatar.jpg",
                IsActive = true,
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var updatedProfileDto = new UserProfileDto
            {
                Id = userId,
                DisplayName = "New Display Name",
                Bio = "Old Bio",
            };

            _mockProfilesRepository
                .Setup(repo => repo.GetProfileAsync(userId))
                .ReturnsAsync(existingProfile);

            _mockMapper
                .Setup(mapper => mapper.Map<UserProfileDto>(It.IsAny<UserProfile>()))
                .Returns(updatedProfileDto);

            // Act
            var result = await _profileCommands.UpdateProfileAsync(userId, request);

            // Assert
            existingProfile.Bio.Should().Be("Old Bio"); // Unchanged
            existingProfile.DisplayName.Should().Be("New Display Name"); // Updated
        }

        [Fact]
        public void UpdateProfileAsync_WhenUserNotFound_ThrowsUserNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new UpdateProfileRequest
            {
                DisplayName = "New Display Name"
            };

            _mockProfilesRepository
                .Setup(repo => repo.GetProfileAsync(userId))
                .ReturnsAsync((UserProfile)null);

            // Act
            Func<Task> act = async () => await _profileCommands.UpdateProfileAsync(userId, request);

            // Assert
            act.Should().ThrowAsync<UserNotFoundException>()
                .WithMessage($"*{userId}*");
        }

        [Fact]
        public void UpdateProfileAsync_WhenUserDeactivated_ThrowsUserDeactivatedException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new UpdateProfileRequest
            {
                DisplayName = "New Display Name"
            };

            var deactivatedProfile = new UserProfile
            {
                Id = userId,
                DisplayName = "Old Display Name",
                IsActive = false
            };

            _mockProfilesRepository
                .Setup(repo => repo.GetProfileAsync(userId))
                .ReturnsAsync(deactivatedProfile);

            // Act
            Func<Task> act = async () => await _profileCommands.UpdateProfileAsync(userId, request);

            // Assert
            act.Should().ThrowAsync<UserDeactivatedException>()
                .WithMessage($"*{userId}*");
        }

        [Fact]
        public async Task UpdateProfileAsync_WithNullRequestProperties_DoesNotUpdateProfile()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new UpdateProfileRequest
            {
                // All properties are null or default
            };

            var existingProfile = new UserProfile
            {
                Id = userId,
                DisplayName = "Old Display Name",
                Bio = "Old Bio",
                AvatarUrl = "https://example.com/avatar.jpg",
                IsActive = true,
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var updatedProfileDto = new UserProfileDto
            {
                Id = userId,
                DisplayName = "Old Display Name", // All unchanged
                Bio = "Old Bio",
                AvatarUrl = "https://example.com/avatar.jpg"
            };

            _mockProfilesRepository
                .Setup(repo => repo.GetProfileAsync(userId))
                .ReturnsAsync(existingProfile);

            _mockMapper
                .Setup(mapper => mapper.Map<UserProfileDto>(It.IsAny<UserProfile>()))
                .Returns(updatedProfileDto);

            // Act
            var result = await _profileCommands.UpdateProfileAsync(userId, request);

            // Assert
            result.Should().BeEquivalentTo(updatedProfileDto);
            // All properties should remain unchanged
            existingProfile.DisplayName.Should().Be("Old Display Name");
            existingProfile.Bio.Should().Be("Old Bio");
            existingProfile.AvatarUrl.Should().Be("https://example.com/avatar.jpg");
            existingProfile.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
    }
}