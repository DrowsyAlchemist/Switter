using AuthService.DTOs.Jwt;
using AuthService.Interfaces;
using AuthService.Interfaces.Jwt;
using AuthService.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace AuthService.Services.Jwt
{
    internal class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRedisService _redisService;
        private readonly JwtSettings _settings;

        public RefreshTokenService(IRedisService redisService, IOptions<JwtSettings> options)
        {
            _redisService = redisService;
            _settings = options.Value;
        }

        public async Task<RefreshTokenData> GenerateRefreshTokenAsync(Guid userId, string remoteIp)
        {
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var expires = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpiryDays);

            var tokenEntity = new RefreshToken
            {
                Token = token,
                Expires = expires,
                CreatedByIp = remoteIp,
                UserId = userId
            };
            var tokenJson = JsonSerializer.Serialize(tokenEntity);
            await _redisService.SetAsync(token, tokenJson, TimeSpan.FromDays(_settings.RefreshTokenDbLifetimeInDays));

            return new RefreshTokenData { Token = token, Expires = expires };
        }

        public async Task<RefreshTokenData> RefreshAsync(string refreshToken, Guid userId, string remoteIp)
        {
            if (string.IsNullOrEmpty(refreshToken))
                throw new ArgumentNullException(nameof(refreshToken));

            var newToken = await GenerateRefreshTokenAsync(userId, remoteIp);
            await RevokeTokenAsync(refreshToken, remoteIp, newToken.Token);
            return newToken;
        }

        public async Task RevokeTokenAsync(string refreshToken, string remoteIp, string? replacedBy = null)
        {
            var tokenJson = await _redisService.GetAsync(refreshToken);

            if (string.IsNullOrEmpty(tokenJson))
                throw new SecurityTokenException("RefreshToken is not found in db.");

            var token = JsonSerializer.Deserialize<RefreshToken>(tokenJson);

            if (token == null)
                throw new Exception($"Deserialize failed. Token: {tokenJson}");
            if (token.IsExpired)
                throw new SecurityTokenException("Refresh token expired");
            if (token.IsRevoked)
                throw new SecurityTokenException("Refresh token revoked");

            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = remoteIp;
            token.ReplacedByToken = replacedBy;

            var revokedTokenJson = JsonSerializer.Serialize(token);
            await _redisService.SetAsync(token.Token, revokedTokenJson);
        }
    }
}
