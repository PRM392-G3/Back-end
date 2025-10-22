using Microsoft.AspNetCore.Mvc;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;

namespace SocialNetworkMobile.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShareController : ControllerBase
    {
        private readonly IShareService _shareService;

        public ShareController(IShareService shareService)
        {
            _shareService = shareService;
        }

        [HttpPost("share")]
        public async Task<ActionResult<ShareResponse>> SharePost([FromBody] SharePostRequest request)
        {
            try
            {
                var result = await _shareService.SharePostAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{postId}/unshare/{userId}")]
        public async Task<ActionResult> UnsharePost(int postId, int userId)
        {
            try
            {
                var result = await _shareService.UnsharePostAsync(userId, postId);
                if (result)
                {
                    return Ok(new { message = "Post unshared successfully" });
                }
                return BadRequest(new { message = "Share not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("post/{postId}")]
        public async Task<ActionResult<List<ShareResponse>>> GetSharesByPost(int postId)
        {
            try
            {
                var shares = await _shareService.GetSharesByPostAsync(postId);
                return Ok(shares);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<ShareResponse>>> GetSharesByUser(int userId)
        {
            try
            {
                var shares = await _shareService.GetSharesByUserAsync(userId);
                return Ok(shares);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{postId}/check/{userId}")]
        public async Task<ActionResult<bool>> HasUserSharedPost(int postId, int userId)
        {
            try
            {
                var hasShared = await _shareService.HasUserSharedPostAsync(userId, postId);
                return Ok(new { hasShared });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{postId}/count")]
        public async Task<ActionResult<int>> GetShareCount(int postId)
        {
            try
            {
                var count = await _shareService.GetShareCountAsync(postId);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
