using AuthService.DTOs.Jwt;

namespace AuthService.Interfaces.Jwt
{
    internal interface IRefreshTokenService
    {
        Task<RefreshTokenData> GenerateRefreshTokenAsync(Guid userId, string remoteIp);
        Task RevokeTokenAsync(string refreshToken, string remoteIp, string? replacedBy = null);
        Task<RefreshTokenData> RefreshAsync(string refreshToken, Guid userId, string remoteIp);
    }
}
