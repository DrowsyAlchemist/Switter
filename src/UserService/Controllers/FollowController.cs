using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.Exceptions.Follows;
using UserService.Exceptions.Profiles;
using UserService.Interfaces.Commands;
using UserService.Interfaces.Queries;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FollowController : ControllerBase
    {
        private readonly IFollowCommands _followCommands;
        private readonly IFollowQueries _followQueries;
        private readonly ILogger<FollowController> _logger;

        public FollowController(IFollowCommands followCommands, IFollowQueries followQueries, ILogger<FollowController> logger)
        {
            _followCommands = followCommands;
            _followQueries = followQueries;
            _logger = logger;
        }

        [HttpPost("{followeeId}")]
        public async Task<IActionResult> Follow(Guid followeeId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                await _followCommands.FollowUserAsync(currentUserId, followeeId);

                _logger.LogInformation("Successfully followed user.\nFollower:{followerId}\nFollowee:{followeeId}", currentUserId, followeeId);
                return Ok(new { message = "Successfully followed user" });
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning(ex, "Follow failed.\nFollowee:{followeeId}", followeeId);
                return NotFound("Followee not found.");
            }
            catch (FollowException ex)
            {
                _logger.LogWarning(ex, "Follow failed.\nFollowee:{followeeId}", followeeId);
                return BadRequest("Already following or cannot follow yourself");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Follow.\nFollowee:{followeeId}", followeeId);
                return StatusCode(500, new { message = "Internal server error" });
            }

        }

        [HttpDelete("{followeeId}")]
        public async Task<IActionResult> Unfollow(Guid followeeId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                await _followCommands.UnfollowUserAsync(currentUserId, followeeId);

                _logger.LogInformation("Successfully unfollowed user.\nFollower:{followerId}\nFollowee:{followeeId}", currentUserId, followeeId);
                return Ok(new { message = "Successfully unfollowed user" });
            }
            catch (FollowNotFoundException ex)
            {
                _logger.LogWarning(ex, "Unfollow failed.\nFollowee:{followeeId}", followeeId);
                return NotFound("Not following this user.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Unollow.\nFollowee:{followeeId}", followeeId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("followers/{userId}")]
        public async Task<IActionResult> GetFollowers(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var followers = await _followQueries.GetFollowersAsync(userId, page, pageSize);

                if (followers.Count == 0)
                    return StatusCode(204, "No followers");

                return Ok(followers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetFollowers.\nUser:{UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("following/{userId}")]
        public async Task<IActionResult> GetFollowing(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var following = await _followQueries.GetFollowingAsync(userId, page, pageSize);

                if (following.Count == 0)
                    return StatusCode(204, "No followings");

                return Ok(following);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetFollowing.\nUser:{UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != null)
                return Guid.Parse(userId!);

            throw new Exception("Current user not found.");
        }
    }
}