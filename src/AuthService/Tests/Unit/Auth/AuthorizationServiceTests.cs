#if DEBUG
using AuthService.DTOs.Auth;
using AuthService.DTOs.Jwt;
using AuthService.Interfaces.Infrastructure;
using AuthService.Interfaces.Jwt;
using AuthService.Models;
using AuthService.Services.Auth;
using Azure.Core;
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
                    PasswordHash = It.IsAny<string>()
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
                var result = await _authorizationService.RegisterAsync(request, remoteIp);

                // Assert
                result.Should().NotBeNull();
                result.Username.Should().Be(request.Username);
                result.Email.Should().Be(request.Email);
                result.AccessToken.Should().Be(expectedAccessToken.Token);
                result.RefreshToken.Should().Be(expectedRefreshToken.Token);
                result.ExpiresAt.Should().Be(expectedAccessToken.Expires);

                _mockUserRepository.Verify(x => x.CreateUserAsync(It.Is<User>(u =>
                    u.Username == request.Username &&
                    u.Email == request.Email &&
                    u.PasswordHash != null)), Times.Once);
            }
        }
    }
}
#endif
