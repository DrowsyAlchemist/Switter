#if DEBUG
using AuthService.DTOs.Auth;
using AuthService.DTOs.Jwt;
using AuthService.Interfaces.Infrastructure;
using AuthService.Interfaces.Jwt;
using AuthService.Models;
using AuthService.Services.Auth;
using FluentAssertions;
using Moq;
using Xunit;

namespace AuthService.Tests.Unit.Auth
{
    public class AuthorizationServiceTests
    {
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly AuthorizationService _authorizationService;

        public AuthorizationServiceTests()
        {
            _mockTokenService = new Mock<ITokenService>();
            _mockUserRepository = new Mock<IUserRepository>();
            _authorizationService = new AuthorizationService(_mockTokenService.Object, _mockUserRepository.Object);
        }

        public class RegisterAsyncTests : AuthorizationServiceTests
        {
            [Fact]
            public async Task RegisterAsync_WhenUserDoesNotExist_ShouldCreateUserAndReturnAuthResponse()
            {
                // Arrange
                var request = new RegisterRequest
                {
                    Username = "testuser",
                    Email = "test@example.com",
                    Password = "password123",
                    ConfirmPassword = "password123"
                };
                var remoteIp = "192.168.1.1";

                _mockUserRepository
                    .Setup(x => x.UserExistsAsync(request.Email, request.Username))
                    .ReturnsAsync(false);

                var expectedUser = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = It.IsNotNull<string>()
                };

                var expectedAccessToken = new AccessTokenData() { Token = "access_token", Expires = It.IsAny<DateTime>() };
                var expectedRefreshToken = new RefreshTokenData() { Token = "refresh_token", Expires = It.IsAny<DateTime>() };

                _mockTokenService
                    .Setup(x => x.GenerateAccessToken(It.IsAny<UserClaims>()))
                    .Returns(expectedAccessToken);

                _mockTokenService
                    .Setup(x => x.GenerateRefreshTokenAsync(It.IsAny<Guid>(), remoteIp))
                    .ReturnsAsync(expectedRefreshToken);

                // Act
                var authResponse = await _authorizationService.RegisterAsync(request, remoteIp);

                // Assert
                authResponse.Should().NotBeNull();
                authResponse.Username.Should().Be(request.Username);
                authResponse.Email.Should().Be(request.Email);
                authResponse.AccessToken.Should().Be(expectedAccessToken.Token);
                authResponse.RefreshToken.Should().Be(expectedRefreshToken.Token);
                authResponse.ExpiresAt.Should().Be(expectedAccessToken.Expires);

                _mockUserRepository.Verify(x => x.CreateUserAsync(It.Is<User>(u =>
                    u.Username == request.Username &&
                    u.Email == request.Email &&
                    u.PasswordHash != null)), Times.Once);
            }

            [Fact]
            public async Task RegisterAsync_WhenUserAlreadyExists_ShouldThrowArgumentException()
            {
                // Arrange
                var request = new RegisterRequest
                {
                    Username = "existinguser",
                    Email = "existing@example.com",
                    Password = "password123",
                    ConfirmPassword = "password123"
                };
                var remoteIp = "192.168.1.1";

                _mockUserRepository
                    .Setup(x => x.UserExistsAsync(request.Email, request.Username))
                    .ReturnsAsync(true);

                // Act & Assert
                await Assert.ThrowsAsync<ArgumentException>(() =>
                    _authorizationService.RegisterAsync(request, remoteIp));

                _mockUserRepository.Verify(x => x.CreateUserAsync(It.IsAny<User>()), Times.Never);
            }
        }

        public class LoginAsyncTests : AuthorizationServiceTests
        {
            [Fact]
            public async Task LoginAsync_WithValidEmailAndPassword_ShouldReturnAuthResponse()
            {
                // Arrange
                var request = new LoginRequest
                {
                    Login = "test@example.com",
                    Password = "password123"
                };
                var remoteIp = "192.168.1.1";

                var existingUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "testuser",
                    Email = "test@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    IsActive = true
                };

                _mockUserRepository
                    .Setup(x => x.GetUserByEmailAsync(request.Login))
                    .ReturnsAsync(existingUser);

                var expectedAccessToken = new AccessTokenData { Token = "access_token", Expires = DateTime.UtcNow.AddHours(1) };
                var expectedRefreshToken = new RefreshTokenData { Token = "refresh_token", Expires = DateTime.UtcNow.AddDays(7) };

                _mockTokenService
                    .Setup(x => x.GenerateAccessToken(It.IsAny<UserClaims>()))
                    .Returns(expectedAccessToken);

                _mockTokenService.Setup(x => x.GenerateRefreshTokenAsync(existingUser.Id, remoteIp))
                    .ReturnsAsync(expectedRefreshToken);

                // Act
                var authResponse = await _authorizationService.LoginAsync(request, remoteIp);

                // Assert
                authResponse.Should().NotBeNull();
                authResponse.UserId.Should().Be(existingUser.Id);
                authResponse.Username.Should().Be(existingUser.Username);
                authResponse.Email.Should().Be(existingUser.Email);
                authResponse.AccessToken.Should().Be(expectedAccessToken.Token);
                authResponse.RefreshToken.Should().Be(expectedRefreshToken.Token);
                authResponse.ExpiresAt.Should().Be(expectedAccessToken.Expires);
            }


            [Fact]
            public async Task LoginAsync_WhenUserNotFound_ShouldThrowUnauthorizedAccessException()
            {
                // Arrange
                var request = new LoginRequest
                {
                    Login = "nonexistent@example.com",
                    Password = "password123"
                };
                var remoteIp = "192.168.1.1";

                _mockUserRepository
                    .Setup(x => x.GetUserByEmailAsync(request.Login))
                    .ReturnsAsync((User?)null);

                _mockUserRepository
                    .Setup(x => x.GetUserByUsernameAsync(request.Login))
                    .ReturnsAsync((User?)null);

                // Act & Assert
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                    _authorizationService.LoginAsync(request, remoteIp));
            }

            [Fact]
            public async Task LoginAsync_WhenInvalidPassword_ShouldThrowUnauthorizedAccessException()
            {
                // Arrange
                var request = new LoginRequest
                {
                    Login = "test@example.com",
                    Password = "wrongpassword"
                };
                var remoteIp = "192.168.1.1";

                var existingUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "testuser",
                    Email = "test@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
                    IsActive = true
                };

                _mockUserRepository
                    .Setup(x => x.GetUserByEmailAsync(request.Login))
                    .ReturnsAsync(existingUser);

                // Act & Assert
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                    _authorizationService.LoginAsync(request, remoteIp));
            }

            [Fact]
            public async Task LoginAsync_WhenAccountDeactivated_ShouldThrowUnauthorizedAccessException()
            {
                // Arrange
                var request = new LoginRequest
                {
                    Login = "test@example.com",
                    Password = "password123"
                };
                var remoteIp = "192.168.1.1";

                var existingUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "testuser",
                    Email = "test@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    IsActive = false
                };

                _mockUserRepository
                    .Setup(x => x.GetUserByEmailAsync(request.Login))
                    .ReturnsAsync(existingUser);

                // Act & Assert
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                    _authorizationService.LoginAsync(request, remoteIp));
            }
        }

        public class RefreshTokenAsyncTests : AuthorizationServiceTests
        {
            [Fact]
            public async Task RefreshTokenAsync_WithValidTokens_ShouldReturnNewAuthResponse()
            {
                // Arrange
                var request = new RefreshRequest
                {
                    AccessToken = "old_access_token",
                    RefreshToken = "old_refresh_token"
                };
                var remoteIp = "192.168.1.1";

                var existingUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "testuser",
                    Email = "test@example.com"
                };

                _mockTokenService
                    .Setup(x => x.ValidateAccessToken(request.AccessToken))
                    .Returns(existingUser.Id);

                _mockUserRepository
                    .Setup(x => x.GetUserByIdAsync(existingUser.Id))
                    .ReturnsAsync(existingUser);

                var newAccessToken = new AccessTokenData { Token = "new_access_token", Expires = DateTime.UtcNow.AddHours(1) };
                var newRefreshToken = new RefreshTokenData { Token = "new_refresh_token", Expires = DateTime.UtcNow.AddDays(1) };

                _mockTokenService
                    .Setup(x => x.GenerateAccessToken(It.IsAny<UserClaims>()))
                    .Returns(newAccessToken);

                _mockTokenService
                    .Setup(x => x.RefreshAsync(request.RefreshToken, existingUser.Id, remoteIp))
                    .ReturnsAsync(newRefreshToken);

                // Act
                var authResponce = await _authorizationService.RefreshTokenAsync(request, remoteIp);

                // Assert
                authResponce.Should().NotBeNull();
                authResponce.UserId.Should().Be(existingUser.Id);
                authResponce.Username.Should().Be(existingUser.Username);
                authResponce.Email.Should().Be(existingUser.Email);
                authResponce.AccessToken.Should().Be(newAccessToken.Token);
                authResponce.RefreshToken.Should().Be(newRefreshToken.Token);
                authResponce.ExpiresAt.Should().Be(newAccessToken.Expires);
            }

            [Fact]
            public async Task RefreshTokenAsync_WhenUserNotFound_ShouldThrowArgumentException()
            {
                // Arrange
                var request = new RefreshRequest
                {
                    AccessToken = "old_access_token",
                    RefreshToken = "old_refresh_token"
                };
                var remoteIp = "192.168.1.1";

                var userId = Guid.NewGuid();

                _mockTokenService
                    .Setup(x => x.ValidateAccessToken(request.AccessToken))
                    .Returns(userId);

                _mockUserRepository
                    .Setup(x => x.GetUserByIdAsync(userId))
                    .ReturnsAsync((User?)null);

                // Act & Assert
                await Assert.ThrowsAsync<ArgumentException>(() =>
                    _authorizationService.RefreshTokenAsync(request, remoteIp));
            }
        }

        public class RevokeTokenAsyncTests : AuthorizationServiceTests
        {
            [Fact]
            public async Task RevokeTokenAsync_ShouldCallTokenService()
            {
                // Arrange
                var refreshToken = "refresh_token";
                var remoteIp = "192.168.1.1";

                _mockTokenService
                    .Setup(x => x.RevokeTokenAsync(refreshToken, remoteIp, null))
                    .Returns(Task.CompletedTask);

                // Act
                await _authorizationService.RevokeTokenAsync(refreshToken, remoteIp);

                // Assert
                _mockTokenService.Verify(x => x.RevokeTokenAsync(refreshToken, remoteIp, null), Times.Once);
            }
        }
    }
}
#endif
