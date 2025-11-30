#if DEBUG
using AuthService.DTOs.Jwt;
using AuthService.Interfaces.Infrastructure;
using AuthService.Models;
using AuthService.Services.Jwt;
using AuthService.Tests.MockServices;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace AuthService.Tests.Unit.Jwt
{
    public class RefreshTokenTests
    {
        private readonly RefreshTokenService _refreshTokenService;
        private readonly Mock<IOptions<JwtSettings>> _mockSettings;
        private readonly IRedisService _mockRedis;

        public RefreshTokenTests()
        {
            var jwtSettings = new JwtSettings
            {
                Secret = "super-secret-key-with-minimum-32-characters-length",
                Issuer = "auth-service",
                Audience = "test",
                AccessTokenExpiryMinutes = 15,
                RefreshTokenExpiryDays = 7,
                RefreshTokenDbLifetimeInDays = 14,
            };
            _mockSettings = new Mock<IOptions<JwtSettings>>();
            _mockSettings.Setup(x => x.Value).Returns(jwtSettings);

            _mockRedis = new MockRedis();
            _refreshTokenService = new RefreshTokenService(_mockRedis, _mockSettings.Object);
        }

        [Fact]
        public async void GenerateRefreshToken_ShouldReturnValidToken()
        {
            // Arrange
            var testUserId = Guid.NewGuid();

            // Act
            RefreshTokenData refreshToken = await _refreshTokenService.GenerateTokenAsync(testUserId, "0.0.0.0");

            // Assert
            refreshToken.Token.Should().NotBeNullOrEmpty();
            refreshToken.Token.Length.Should().Be(24);
            refreshToken.Token.Should().MatchRegex("^[A-z0-9+/=]{24}$");
        }

        [Fact]
        public async void RefreshToken_WithValidToken_ShouldReturnValidToken()
        {
            // Arrange 
            var testUserId = Guid.NewGuid();
            var validToken = await _refreshTokenService.GenerateTokenAsync(testUserId, "0.0.0.0");

            // Act
            var resultTokenData = await _refreshTokenService.RefreshAsync(validToken.Token, testUserId, "0.0.0.0");
            var newToken = resultTokenData.Token;

            // Assert
            newToken.Should().NotBeNullOrEmpty();
            newToken.Should().NotBeNullOrEmpty();
            newToken.Length.Should().Be(24);
            newToken.Should().MatchRegex("^[A-z0-9+/=]{24}$");
        }

        [Fact]
        public async void RefreshToken_WithRevokedToken_ShouldThrowSecurityTokenException()
        {
            // Arrange 
            var testUserId = Guid.NewGuid();
            var token = await _refreshTokenService.GenerateTokenAsync(testUserId, "0.0.0.0");
            await _refreshTokenService.RevokeAsync(token.Token, "0.0.0.0");

            // Act and Assert
            await Assert.ThrowsAsync<SecurityTokenException>(() =>
                _refreshTokenService.RefreshAsync(token.Token, testUserId, "0.0.0.0"));
        }
    }
}
# endif