using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.Exceptions.Blocks;
using UserService.Exceptions.Profiles;
using UserService.Interfaces;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BlockController : ControllerBase
    {
        private readonly IBlockService _blockService;
        private readonly ILogger<BlockController> _logger;

        public BlockController(IBlockService blockService, ILogger<BlockController> logger)
        {
            _blockService = blockService;
            _logger = logger;
        }

        [HttpPost("{userToBlock}")]
        public async Task<IActionResult> Block(Guid userToBlock)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                await _blockService.BlockAsync(currentUserId, userToBlock);

                _logger.LogInformation("Successfully blocked user.\nBlocker:{blockerId}\nBlocked:{blockedId}", currentUserId, userToBlock);
                return Ok(new { message = "Successfully blocked user" });
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning(ex, "Blocking failed.\nUser to block:{id}", userToBlock);
                return NotFound("User not found.");
            }
            catch (BlockException ex)
            {
                _logger.LogWarning(ex, "Blocking failed.\nUser to block:{id}", userToBlock);
                return BadRequest("Already blocked or cannot block yourself");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Blocking.\nUser to block:{id}", userToBlock);
                return StatusCode(500, new { message = "Internal server error" });
            }

        }

        [HttpDelete("{blockedId}")]
        public async Task<IActionResult> Unblock(Guid blockedId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                await _blockService.UnblockAsync(currentUserId, blockedId);

                _logger.LogInformation("Successfully unblock user.\nBlocker:{blockerId}\nBlocked:{blockedId}", currentUserId, blockedId);
                return Ok(new { message = "Successfully unblock user" });
            }
            catch (BlockNotFoundException ex)
            {
                _logger.LogWarning(ex, "Unblocking failed.\nUser to unblock:{blockedId}", blockedId);
                return NotFound("User is not blocked.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Unblocking.\nUser to unblock:{blockedId}", blockedId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("blocked/{userId}")]
        public async Task<IActionResult> GetBlocked(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (userId != currentUserId)
                    return Forbid();

                var blocked = await _blockService.GetBlockedAsync(userId, page, pageSize);

                if (blocked.Count == 0)
                    return StatusCode(204, "No blocked users.");

                return Ok(blocked);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GetBlocked.\nUser:{UserId}", userId);
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