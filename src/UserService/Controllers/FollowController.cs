using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.DTOs;
using UserService.Interfaces;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FollowController : ControllerBase
    {
        private readonly IFollowService _followService;
        private readonly ILogger<FollowController> _logger;

        public FollowController(IFollowService followService, ILogger<FollowController> logger)
        {
            _followService = followService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Follow([FromBody] FollowRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId.HasValue == false)
                    throw new Exception("Current user not found.");

                await _followService.FollowUserAsync(currentUserId.Value, request.FolloweeId);

                _logger.LogInformation("Successfully followed user.\nFollower:{followerId}\nFollowee:{followeeId}", currentUserId.Value, request.FolloweeId);
                return Ok(new { message = "Successfully followed user" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Follow failed.\nFollowee:{followeeId}", request.FolloweeId);
                return BadRequest("Already following or cannot follow yourself");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Follow.\nFollowee:{followeeId}", request.FolloweeId);
                return StatusCode(500, new { message = "Internal server error" });
            }

        }

        [HttpDelete("{followeeId}")]
        public async Task<IActionResult> Unfollow(Guid followeeId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId.HasValue == false)
                    throw new Exception("Current user not found.");

                await _followService.UnfollowUserAsync(currentUserId.Value, followeeId);

                _logger.LogInformation("Successfully unfollowed user.\nFollower:{followerId}\nFollowee:{followeeId}", currentUserId.Value, followeeId);
                return Ok(new { message = "Successfully unfollowed user" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Unfollow failed.\nFollowee:{followeeId}", followeeId);
                return BadRequest("Not following this user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Unollow.\nFollowee:{followeeId}", followeeId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("followers/{userId}")]
        public async Task<IActionResult> GetFollowers(Guid userId, [FromQuery] int page = 1)
        {
            try
            {
                var followers = await _followService.GetFollowersAsync(userId, page);
                return Ok(followers);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "GetFollowers failed.\nUserId:{UserId}", userId);
                return NotFound("User not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetFollowers.\nUser:{UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("following/{userId}")]
        public async Task<IActionResult> GetFollowing(Guid userId, [FromQuery] int page = 1)
        {
            try
            {
                var following = await _followService.GetFollowingAsync(userId, page);
                return Ok(following);
            }

            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "GetFollowing failed.\nUserId:{UserId}", userId);
                return NotFound("User not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetFollowing.\nUser:{UserId}", userId);
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