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
            if (IsPaginationCorrect(page, pageSize) == false)
            {
                _logger.LogWarning("GetFollowers failed.\nPagination is incorrect. Page: {page}, Size: {size}", page, pageSize);
                return BadRequest("Pagination is incorrect.");
            }
            try
            {
                var followers = await _followQueries.GetFollowersAsync(userId, page, pageSize);

                if (followers.Any() == false)
                    return StatusCode(204, "No followers");

                return Ok(followers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetFollowers.\nUser:{UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("followings/{userId}")]
        public async Task<IActionResult> GetFollowings(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                if (IsPaginationCorrect(page, pageSize) == false)
                {
                    _logger.LogWarning("GetFollowings failed.\nPagination is incorrect. Page: {page}, Size: {size}", page, pageSize);
                    return BadRequest("Pagination is incorrect.");
                }
                var following = await _followQueries.GetFollowingsAsync(userId, page, pageSize);

                if (following.Any() == false)
                    return StatusCode(204, "No followings");

                return Ok(following);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetFollowing.\nUser:{UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("followerIds/{userId}")]
        public async Task<IActionResult> GetFollowerIds(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                if (IsPaginationCorrect(page, pageSize) == false)
                {
                    _logger.LogWarning("GetFollowerIds failed.\nPagination is incorrect. Page: {page}, Size: {size}", page, pageSize);
                    return BadRequest("Pagination is incorrect.");
                }
                var followers = await _followQueries.GetFollowerIdsAsync(userId, page, pageSize);

                if (followers.Any() == false)
                    return StatusCode(204, "No followers");

                return Ok(followers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetFollowers.\nUser:{UserId}", userId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("followingIds/{userId}")]
        public async Task<IActionResult> GetFollowingIds(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (IsPaginationCorrect(page, pageSize) == false)
            {
                _logger.LogWarning("GetFollowers failed.\nPagination is incorrect. Page: {page}, Size: {size}", page, pageSize);
                return BadRequest("GetFollowingIds is incorrect.");
            }
            try
            {
                var following = await _followQueries.GetFollowingIdsAsync(userId, page, pageSize);

                if (following.Any() == false)
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

        private bool IsPaginationCorrect(int page, int pageSize)
        {
            return page > 0 && pageSize > 0;
        }
    }
}