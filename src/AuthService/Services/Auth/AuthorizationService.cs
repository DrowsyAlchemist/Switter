using AuthService.DTOs.Auth;
using AuthService.DTOs.Jwt;
using AuthService.Interfaces.Auth;
using AuthService.Interfaces.Infrastructure;
using AuthService.Interfaces.Jwt;
using AuthService.Models;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services.Auth
{
    internal class AuthorizationService : IAuthorizationService
    {
        private readonly ITokenService _tokenService;
        private readonly IUserRepository _userService;

        public AuthorizationService(ITokenService tokenService, IUserRepository userService)
        {
            _tokenService = tokenService;
            _userService = userService;
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

            var authResponse = await GenerateAuthResponseAsync(user, remoteIp);

            await _userService.CreateUserAsync(user);
            return authResponse;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, string remoteIp)
        {
            var user = await _userService.GetUserByEmailAsync(request.Login)
                  ?? await _userService.GetUserByUsernameAsync(request.Login);

            if (user == null)
                throw new UnauthorizedAccessException($"User with login {request.Login} in not found");

            if (BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash) == false)
                throw new UnauthorizedAccessException($"Invalid password. Login: {request.Login}");

            if (!user.IsActive)
                throw new UnauthorizedAccessException($"Account is deactivated. Login: {request.Login}");

            var authResponse = await GenerateAuthResponseAsync(user, remoteIp);
            return authResponse;
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshRequest request, string remoteIp)
        {
            var userId = _tokenService.ValidateAccessToken(request.AccessToken);
            if (userId == null)
                throw new SecurityTokenException("Invalid access token");

            var user = await _userService.GetUserByIdAsync(userId.Value);
            if (user == null)
                throw new ArgumentException("User not found");

            var userClaims = new UserClaims { Id = user.Id, Name = user.Username, Email = user.Email };
            var newAccessToken = _tokenService.GenerateAccessToken(userClaims);
            var newRefreshToken = await _tokenService.RefreshAsync(request.RefreshToken, userId.Value, remoteIp);

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
            await _tokenService.RevokeTokenAsync(refreshToken, remoteIp);
        }

        private async Task<AuthResponse> GenerateAuthResponseAsync(User user, string remoteIp)
        {
            var userClaims = new UserClaims { Id = user.Id, Name = user.Username, Email = user.Email };
            var accessToken = _tokenService.GenerateAccessToken(userClaims);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, remoteIp);

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