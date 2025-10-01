using AuthService.DTOs;
using AuthService.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    internal class AuthController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthorizationService authorizationService, ILogger<AuthController> logger)
        {
            _authorizationService = authorizationService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            try
            {
                var authResponse = await _authorizationService.RegisterAsync(registerRequest, GetRemoteIp());
                return Ok(authResponse);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { message = "Internal server error" });
            }

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest authRequest)
        {
            try
            {
                var response = await _authorizationService.LoginAsync(authRequest, GetRemoteIp());
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var response = await _authorizationService.RefreshTokenAsync(request, GetRemoteIp());
                return Ok(response);
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] string refreshToken)
        {
            try
            {
                await _authorizationService.RevokeTokenAsync(refreshToken, GetRemoteIp());
                return Ok(new { message = "Token revoked successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private string GetRemoteIp() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}
