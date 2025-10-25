using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;

namespace SocialNetworkMobile.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShareController : ControllerBase
    {
        private readonly IShareService _shareService;

        public ShareController(IShareService shareService)
        {
            _shareService = shareService;
        }

        [HttpPost("share")]
        public async Task<ActionResult> SharePost([FromBody] SharePostRequest request)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                if (currentUserId == 0)
                    return Unauthorized(new { error = "Invalid user token" });

                // Override userId with current user
                request.UserId = currentUserId;

                var result = await _shareService.SharePostAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpDelete("unshare/{postId}")]
        public async Task<ActionResult> UnsharePost(int postId)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                if (currentUserId == 0)
                    return Unauthorized(new { error = "Invalid user token" });

                var result = await _shareService.UnsharePostAsync(currentUserId, postId);
                if (!result)
                    return NotFound(new { error = "Share not found" });

                return Ok(new { message = "Post unshared successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("check/{postId}")]
        public async Task<ActionResult> IsPostShared(int postId)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                if (currentUserId == 0)
                    return Unauthorized(new { error = "Invalid user token" });

                var isShared = await _shareService.IsPostSharedByUserAsync(currentUserId, postId);
                return Ok(new { isShared });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("post/{postId}")]
        public async Task<ActionResult> GetPostShares(int postId)
        {
            try
            {
                var shares = await _shareService.GetPostSharesAsync(postId);
                return Ok(shares);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult> GetUserShares(int userId)
        {
            try
            {
                var shares = await _shareService.GetUserSharesAsync(userId);
                return Ok(shares);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("count/{postId}")]
        public async Task<ActionResult> GetPostShareCount(int postId)
        {
            try
            {
                var count = await _shareService.GetPostShareCountAsync(postId);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}