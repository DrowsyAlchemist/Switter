using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.DTOs;
using UserService.Exceptions;
using UserService.Interfaces.Commands;
using UserService.Interfaces.Queries;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfileController : ControllerBase
    {
        private readonly IProfileCommands _profileCommands;
        private readonly IProfileQueries _profileQueries;
        private readonly ILogger<UserProfileController> _logger;

        public UserProfileController(IProfileCommands profileCommands, IProfileQueries profileQueries, ILogger<UserProfileController> logger)
        {
            _profileCommands = profileCommands;
            _profileQueries = profileQueries;
            _logger = logger;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetProfile(Guid userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var profile = await _profileQueries.GetProfileAsync(userId, currentUserId);
                _logger.LogInformation("Profile successfully returned.\nUserId: {userId}\nCurrentUserId:{currentUserId}", userId, currentUserId);
                return Ok(profile);
            }
            catch (UserServiceException ex)
            {
                _logger.LogWarning(ex, "GetProfile failed.\nUserId: {userId}", userId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetProfile. UserId: {id}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                if (currentUserId.HasValue == false)
                    throw new Exception("Current user not found.");

                var profile = await _profileQueries.GetProfileAsync(currentUserId.Value);
                _logger.LogInformation("Profile successfully returned.\nCurrentUserId:{id}", currentUserId);
                return Ok(profile);
            }
            catch (UserServiceException ex)
            {
                _logger.LogWarning(ex, "GetMyProfile failed.");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetMyProfile.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId.HasValue == false)
                    throw new Exception("Current user not found.");

                var updatedProfile = await _profileCommands.UpdateProfileAsync(currentUserId.Value, request);
                _logger.LogInformation("Profile updated.\nUserId: {userId}", currentUserId);
                return Ok(updatedProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during UpdateProfile.");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var users = await _profileQueries.SearchUsersAsync(query, page, pageSize);
                _logger.LogInformation("UsersList sent.\nQuery: {query}", query);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during SearchUsers.\nQuery: {query}", query);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return null;

            return Guid.Parse(userId!);
        }
    }
}
