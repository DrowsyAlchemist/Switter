using AuthService.DTOs;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    internal class AuthController : Controller
    {
        private readonly AuthorizationService _authorizationService;
        private readonly ILogger<AuthController> _logger;
        private readonly HttpContext _httpContext;

        public AuthController(AuthorizationService authorizationService, ILogger<AuthController> logger, HttpContext httpContext)
        {
            _authorizationService = authorizationService;
            _logger = logger;
            _httpContext = httpContext;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            loginRequest.IpAddress = _httpContext.Request.Host.Host;
            
            try
            {
                var response = await _authorizationService.LoginAsync(loginRequest);
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
    }
}
