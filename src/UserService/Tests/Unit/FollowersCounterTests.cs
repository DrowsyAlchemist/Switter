using AutoMapper;
using FluentAssertions;
using Moq;
using UserService.DTOs;
using UserService.Exceptions.Profiles;
using UserService.Interfaces.Data;
using UserService.Interfaces.Infrastructure;
using UserService.Models;
using UserService.Services;
using Xunit;

namespace UserService.Tests.Unit
{
    public class FollowersCounterTests
    {
        private readonly Mock<IProfilesRepository> ProfilesRepositoryMock;
        private readonly Mock<IRedisService> RedisServiceMock;
        private readonly Mock<IMapper> MapperMock;
        private readonly FollowersCounter FollowersCounter;

        public FollowersCounterTests()
        {
            ProfilesRepositoryMock = new Mock<IProfilesRepository>();
            RedisServiceMock = new Mock<IRedisService>();
            MapperMock = new Mock<IMapper>();

            FollowersCounter = new FollowersCounter(
                ProfilesRepositoryMock.Object,
                RedisServiceMock.Object,
                MapperMock.Object
            );
        }

        [Fact]
        public async Task IncrementCounter_WithValidUsers_IncrementsCountersAndClearsCache()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            var followerProfile = new UserProfile
            {
                Id = followerId,
                FollowingCount = 5,
                Followers = new List<Follow>(),
                Following = new List<Follow>()
            };

            var followeeProfile = new UserProfile
            {
                Id = followeeId,
                FollowersCount = 10,
                Followers = new List<Follow>(),
                Following = new List<Follow>()
            };

            ProfilesRepositoryMock
                .Setup(x => x.GetProfileAsync(followerId))
                .ReturnsAsync(followerProfile);

            ProfilesRepositoryMock
                .Setup(x => x.GetProfileAsync(followeeId))
                .ReturnsAsync(followeeProfile);

            ProfilesRepositoryMock
                .Setup(x => x.UpdateProfileAsync(It.IsAny<UserProfile>()))
                .ReturnsAsync(It.IsAny<UserProfile>());

            RedisServiceMock
                .Setup(x => x.RemoveAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await FollowersCounter.IncrementCounter(followerId, followeeId);

            // Assert
            followerProfile.FollowingCount.Should().Be(6);
            followeeProfile.FollowersCount.Should().Be(11);

            RedisServiceMock.Verify(x => x.RemoveAsync(It.Is<string>(k => k.Contains(followerId.ToString()))), Times.Once);
            RedisServiceMock.Verify(x => x.RemoveAsync(It.Is<string>(k => k.Contains(followeeId.ToString()))), Times.Once);

            ProfilesRepositoryMock.Verify(x => x.UpdateProfileAsync(It.Is<UserProfile>(p =>
                p.Id == followerId && p.FollowingCount == 6)), Times.Once);
            ProfilesRepositoryMock.Verify(x => x.UpdateProfileAsync(It.Is<UserProfile>(p =>
                p.Id == followeeId && p.FollowersCount == 11)), Times.Once);
        }

        [Fact]
        public async Task DecrementCounter_WithValidUsers_DecrementsCountersAndClearsCache()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            var followerProfile = new UserProfile
            {
                Id = followerId,
                FollowingCount = 5,
                Followers = new List<Follow>(),
                Following = new List<Follow>()
            };

            var followeeProfile = new UserProfile
            {
                Id = followeeId,
                FollowersCount = 10,
                Followers = new List<Follow>(),
                Following = new List<Follow>()
            };

            ProfilesRepositoryMock
                .Setup(x => x.GetProfileAsync(followerId))
                .ReturnsAsync(followerProfile);

            ProfilesRepositoryMock
                .Setup(x => x.GetProfileAsync(followeeId))
                .ReturnsAsync(followeeProfile);

            ProfilesRepositoryMock
                .Setup(x => x.UpdateProfileAsync(It.IsAny<UserProfile>()))
                .ReturnsAsync(It.IsAny<UserProfile>());

            RedisServiceMock
                .Setup(x => x.RemoveAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await FollowersCounter.DecrementCounter(followerId, followeeId);

            // Assert
            followerProfile.FollowingCount.Should().Be(4);
            followeeProfile.FollowersCount.Should().Be(9);

            RedisServiceMock.Verify(x => x.RemoveAsync(It.Is<string>(k => k.Contains(followerId.ToString()))), Times.Once);
            RedisServiceMock.Verify(x => x.RemoveAsync(It.Is<string>(k => k.Contains(followeeId.ToString()))), Times.Once);

            ProfilesRepositoryMock.Verify(x => x.UpdateProfileAsync(It.Is<UserProfile>(p =>
                p.Id == followerId && p.FollowingCount == 4)), Times.Once);
            ProfilesRepositoryMock.Verify(x => x.UpdateProfileAsync(It.Is<UserProfile>(p =>
                p.Id == followeeId && p.FollowersCount == 9)), Times.Once);
        }

        [Fact]
        public async Task IncrementCounter_WhenFollowerNotFound_ThrowsUserNotFoundException()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            ProfilesRepositoryMock
                .Setup(x => x.GetProfileAsync(followerId))
                .ReturnsAsync((UserProfile)null);

            ProfilesRepositoryMock
                .Setup(x => x.GetProfileAsync(followeeId))
                .ReturnsAsync(new UserProfile { Id = followeeId });

            // Act & Assert
            await Assert.ThrowsAsync<UserNotFoundException>(() =>
                FollowersCounter.IncrementCounter(followerId, followeeId));

            ProfilesRepositoryMock.Verify(x => x.UpdateProfileAsync(It.IsAny<UserProfile>()), Times.Never);
        }

        [Fact]
        public async Task IncrementCounter_WhenFolloweeNotFound_ThrowsUserNotFoundException()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            ProfilesRepositoryMock
                .Setup(x => x.GetProfileAsync(followerId))
                .ReturnsAsync(new UserProfile { Id = followerId });

            ProfilesRepositoryMock
                .Setup(x => x.GetProfileAsync(followeeId))
                .ReturnsAsync((UserProfile)null);

            // Act & Assert
            await Assert.ThrowsAsync<UserNotFoundException>(() =>
                FollowersCounter.IncrementCounter(followerId, followeeId));

            ProfilesRepositoryMock.Verify(x => x.UpdateProfileAsync(It.IsAny<UserProfile>()), Times.Never);
        }

        [Fact]
        public async Task ForceUpdateCountersForUserAsync_WithValidUser_UpdatesCountersFromCollectionsAndClearsCache()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var followers = new List<Follow>
        {
            new Follow { Id = Guid.NewGuid() },
            new Follow { Id = Guid.NewGuid() },
            new Follow { Id = Guid.NewGuid() }
        };

            var following = new List<Follow>
        {
            new Follow { Id = Guid.NewGuid() },
            new Follow { Id = Guid.NewGuid() }
        };

            var user = new UserProfile
            {
                Id = userId,
                Followers = followers,
                Following = following,
                FollowersCount = 0,
                FollowingCount = 0
            };

            var expectedDto = new UserProfileDto
            {
                Id = userId,
                FollowersCount = 3,
                FollowingCount = 2
            };

            ProfilesRepositoryMock
                .Setup(x => x.GetProfileAsync(userId))
                .ReturnsAsync(user);

            ProfilesRepositoryMock
                .Setup(x => x.UpdateProfileAsync(It.IsAny<UserProfile>()))
                .ReturnsAsync(It.IsAny<UserProfile>());

            RedisServiceMock
                .Setup(x => x.RemoveAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            MapperMock
                .Setup(x => x.Map<UserProfileDto>(It.IsAny<UserProfile>()))
                .Returns(expectedDto);

            // Act
            var result = await FollowersCounter.ForceUpdateCountersForUserAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.FollowersCount.Should().Be(3);
            result.FollowingCount.Should().Be(2);

            user.FollowersCount.Should().Be(3);
            user.FollowingCount.Should().Be(2);

            ProfilesRepositoryMock.Verify(x => x.GetProfileAsync(userId), Times.Once);
            ProfilesRepositoryMock.Verify(x => x.UpdateProfileAsync(It.Is<UserProfile>(p =>
                p.FollowersCount == 3 && p.FollowingCount == 2)), Times.Once);
            RedisServiceMock.Verify(x => x.RemoveAsync(It.Is<string>(k => k.Contains(userId.ToString()))), Times.Once);
            MapperMock.Verify(x => x.Map<UserProfileDto>(user), Times.Once);
        }

        [Fact]
        public async Task ForceUpdateCountersForUserAsync_WhenUserNotFound_ThrowsUserNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            ProfilesRepositoryMock
                .Setup(x => x.GetProfileAsync(userId))
                .ReturnsAsync((UserProfile?)null);

            // Act & Assert
            await Assert.ThrowsAsync<UserNotFoundException>(() =>
                FollowersCounter.ForceUpdateCountersForUserAsync(userId));

            ProfilesRepositoryMock.Verify(x => x.UpdateProfileAsync(It.IsAny<UserProfile>()), Times.Never);
            MapperMock.Verify(x => x.Map<UserProfileDto>(It.IsAny<UserProfile>()), Times.Never);
        }

        [Fact]
        public async Task ForceUpdateCountersForUserAsync_WithEmptyCollections_SetsCountersToZero()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var user = new UserProfile
            {
                Id = userId,
                Followers = new List<Follow>(),
                Following = new List<Follow>(),
                FollowersCount = 5,
                FollowingCount = 3
            };

            var expectedDto = new UserProfileDto
            {
                Id = userId,
                FollowersCount = 0,
                FollowingCount = 0
            };

            ProfilesRepositoryMock
                .Setup(x => x.GetProfileAsync(userId))
                .ReturnsAsync(user);

            ProfilesRepositoryMock
                .Setup(x => x.UpdateProfileAsync(It.IsAny<UserProfile>()))
                .ReturnsAsync(It.IsAny<UserProfile>());

            MapperMock
                .Setup(x => x.Map<UserProfileDto>(It.IsAny<UserProfile>()))
                .Returns(expectedDto);

            // Act
            var result = await FollowersCounter.ForceUpdateCountersForUserAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.FollowersCount.Should().Be(0);
            result.FollowingCount.Should().Be(0);

            user.FollowersCount.Should().Be(0);
            user.FollowingCount.Should().Be(0);

            ProfilesRepositoryMock.Verify(x => x.UpdateProfileAsync(It.Is<UserProfile>(p =>
                p.FollowersCount == 0 && p.FollowingCount == 0)), Times.Once);
        }

        [Fact]
        public async Task DecrementCounter_WithZeroCounters_ThrowsInvalidOperationException()
        {
            // Arrange
            var followerId = Guid.NewGuid();
            var followeeId = Guid.NewGuid();

            var followerProfile = new UserProfile
            {
                Id = followerId,
                FollowingCount = 0,
                Followers = new List<Follow>(),
                Following = new List<Follow>()
            };

            var followeeProfile = new UserProfile
            {
                Id = followeeId,
                FollowersCount = 0,
                Followers = new List<Follow>(),
                Following = new List<Follow>()
            };

            ProfilesRepositoryMock
                .Setup(x => x.GetProfileAsync(followerId))
                .ReturnsAsync(followerProfile);

            ProfilesRepositoryMock
                .Setup(x => x.GetProfileAsync(followeeId))
                .ReturnsAsync(followeeProfile);

            ProfilesRepositoryMock
                .Setup(x => x.UpdateProfileAsync(It.IsAny<UserProfile>()))
                .ReturnsAsync(It.IsAny<UserProfile>());

            RedisServiceMock
                .Setup(x => x.RemoveAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                FollowersCounter.DecrementCounter(followerId, followeeId));

            ProfilesRepositoryMock.Verify(x => x.UpdateProfileAsync(It.IsAny<UserProfile>()), Times.Never);
        }
    }
}
