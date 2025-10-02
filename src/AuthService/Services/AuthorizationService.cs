using AuthService.DTOs;
using AuthService.Interfaces;
using AuthService.Models;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services
{
    internal class AuthorizationService : IAuthorizationService
    {
        private readonly IAccessTokenService _accessTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IUserService _userService;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(IAccessTokenService accessTokenService, IRefreshTokenService refreshTokenService,
                          IUserService userService, ILogger<AuthorizationService> logger)
        {
            _accessTokenService = accessTokenService;
            _refreshTokenService = refreshTokenService;
            _userService = userService;
            _logger = logger;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string remoteIp)
        {
            if (await _userService.UserExistsAsync(request.Email, request.Username))
                throw new ArgumentException("User with this email or username already exists");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash
            };
            await _userService.CreateUserAsync(user);

            var authResponse = await GenerateAuthResponseAsync(user, remoteIp);
            _logger.LogInformation("User registered: {Username} ({Email})", user.Username, user.Email);
            return authResponse;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, string remoteIp)
        {
            var user = await _userService.GetUserByEmailAsync(request.Login)
                  ?? await _userService.GetUserByUsernameAsync(request.Login);

            if (user == null)
                throw new UnauthorizedAccessException($"User with login {request.Login} in not found");

            if (BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash) == false)
                throw new UnauthorizedAccessException("Invalid password");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is deactivated");

            var authResponse = await GenerateAuthResponseAsync(user, remoteIp);
            _logger.LogInformation("User logged in: {Username}", user.Username);
            return authResponse;
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string remoteIp)
        {
            var userId = _accessTokenService.ValidateAccessToken(request.AccessToken);
            if (userId == null)
                throw new SecurityTokenException("Invalid access token");

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null)
                throw new ArgumentException("User not found");

            var newAccessToken = _accessTokenService.GenerateAccessToken(user);
            var newRefreshToken = await _refreshTokenService.RefreshAsync(request.RefreshToken, userId.Value, remoteIp);

            _logger.LogInformation("Token refreshed for {Username}", user.Username);
            return new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                AccessToken = newAccessToken.Token,
                RefreshToken = newRefreshToken.Token,
                ExpiresAt = newAccessToken.Expires
            };
        }

        public async Task RevokeTokenAsync(string refreshToken, string remoteIp)
        {
            await _refreshTokenService.RevokeTokenAsync(refreshToken, remoteIp);
        }

        private async Task<AuthResponse> GenerateAuthResponseAsync(User user, string remoteIp)
        {
            var accessToken = _accessTokenService.GenerateAccessToken(user);
            var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, remoteIp);

            return new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                AccessToken = accessToken.Token,
                RefreshToken = refreshToken.Token,
                ExpiresAt = accessToken.Expires
            };
        }
    }
}