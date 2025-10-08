using AuthService.DTOs.Jwt;

namespace AuthService.Interfaces.Jwt
{
    internal interface IRefreshTokenService
    {
        Task<RefreshTokenData> GenerateTokenAsync(Guid userId, string remoteIp);
        Task RevokeAsync(string refreshToken, string remoteIp, string? replacedBy = null);
        Task<RefreshTokenData> RefreshAsync(string refreshToken, Guid userId, string remoteIp);
    }
}
