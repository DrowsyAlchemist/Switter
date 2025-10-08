#if DEBUG
using AuthService.DTOs.Jwt;
using AuthService.Models;
using AuthService.Services.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace AuthService.Tests.UnitTests
{
    [CollectionDefinition("ServiceTests")]
    public class AccessTokenServiceTests
    {
        private readonly AccessTokenService _accessTokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly Mock<IOptions<JwtSettings>> _mockSettings;

        public AccessTokenServiceTests()
        {
            _jwtSettings = new JwtSettings
            {
                Secret = "super-secret-key-with-minimum-32-characters-length",
                Issuer = "auth-service",
                Audience = "portfolio-app",
                AccessTokenExpiryMinutes = 15,
                RefreshTokenExpiryDays = 7,
                RefreshTokenDbLifetimeInDays = 14,
            };
            _mockSettings = new Mock<IOptions<JwtSettings>>();
            _mockSettings.Setup(x => x.Value).Returns(_jwtSettings);

            _accessTokenService = new AccessTokenService(_mockSettings.Object);
        }

        [Fact]
        public void GenerateAccessToken_WithValidUser_ShouldReturnValidToken()
        {
            // Arrange
            var user = new UserClaims { Id = Guid.NewGuid(), Email = "test@example.com", Name = "User" };

            // Act
            var token = _accessTokenService.GenerateToken(user);

            // Assert
            token.Should().NotBeNull();
            token.Token.Should().NotBeNullOrEmpty();

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token.Token);

            jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value
                .Should().Be(user.Id.ToString());
            jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value
                .Should().Be(user.Email);
            jwtToken.Claims.First(c => c.Type == ClaimTypes.Name).Value
                .Should().Be(user.Name);

            jwtToken.Issuer.Should().Be(_jwtSettings.Issuer);
            jwtToken.Audiences.First().Should().Be(_jwtSettings.Audience);
        }

        [Fact]
        public void ValidateToken_WithValidToken_ShouldReturnUserId()
        {
            // Arrange
            var user = new UserClaims { Id = Guid.NewGuid(), Email = "test@example.com", Name = "User" };
            var token = _accessTokenService.GenerateToken(user);

            // Act
            var result = _accessTokenService.ValidateToken(token.Token);
            var userId = result.UserId;

            // Assert
            userId.Should().NotBeNull();
            userId.Should().Be(user.Id);
        }

        [Fact]
        public void ValidateToken_WithExpiredToken_ShouldThrowSecurityTokenException()
        {
            // Arrange
            var expiredSettings = new JwtSettings
            {
                Secret = _jwtSettings.Secret,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                AccessTokenExpiryMinutes = -1,
            };

            var expiredOptions = Mock.Of<IOptions<JwtSettings>>(x => x.Value == expiredSettings);
            var expiredAccessTokenService = new AccessTokenService(expiredOptions);

            var user = new UserClaims { Id = Guid.NewGuid(), Email = "test@example.com", Name = "User" };
            var expiredToken = expiredAccessTokenService.GenerateToken(user);

            // Act & Assert
            Assert.Throws<SecurityTokenException>(() =>
                _accessTokenService.ValidateToken(expiredToken.Token));
        }

        [Fact]
        public void ValidateToken_WithInvalidToken_ShouldThrowSecurityTokenException()
        {
            // Arrange
            var user = new UserClaims { Id = Guid.NewGuid(), Email = "test@example.com", Name = "User" };
            var token = _accessTokenService.GenerateToken(user);
            var charArray = token.Token.ToCharArray();
            charArray[10] = 'a';
            charArray[11] = 'b';
            var invalidToken = new string(charArray);

            // Act & Assert
            Assert.Throws<SecurityTokenException>(() =>
                _accessTokenService.ValidateToken(invalidToken));
        }
    }
}
#endif