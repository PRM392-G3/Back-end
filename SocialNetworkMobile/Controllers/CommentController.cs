using Microsoft.AspNetCore.Mvc;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;
using Microsoft.AspNetCore.Authorization;

namespace SocialNetworkMobile.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CommentResponse>> CreateComment([FromBody] CreateCommentRequest request)
        {
            try
            {
                var comment = await _commentService.CreateCommentAsync(request);
                return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CommentResponse>> GetComment(int id)
        {
            try
            {
                var comment = await _commentService.GetCommentByIdAsync(id);
                return Ok(comment);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("post/{postId}")]
        public async Task<ActionResult<List<CommentResponse>>> GetCommentsByPost(int postId)
        {
            try
            {
                var comments = await _commentService.GetCommentsByPostIdAsync(postId);
                return Ok(comments);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{commentId}/replies")]
        public async Task<ActionResult<List<CommentResponse>>> GetReplies(int commentId)
        {
            try
            {
                var replies = await _commentService.GetRepliesByCommentIdAsync(commentId);
                return Ok(replies);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<CommentResponse>> UpdateComment(int id, [FromBody] UpdateCommentRequest request)
        {
            try
            {
                var comment = await _commentService.UpdateCommentAsync(id, request);
                return Ok(comment);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteComment(int id)
        {
            try
            {
                var result = await _commentService.DeleteCommentAsync(id);
                if (result)
                    return NoContent();
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{commentId}/like/{userId}")]
        [Authorize]
        public async Task<ActionResult> LikeComment(int commentId, int userId)
        {
            try
            {
                var result = await _commentService.LikeCommentAsync(userId, commentId);
                if (result)
                    return Ok(new { message = "Comment liked successfully" });
                return BadRequest("Unable to like comment");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{commentId}/like/{userId}")]
        [Authorize]
        public async Task<ActionResult> UnlikeComment(int commentId, int userId)
        {
            try
            {
                var result = await _commentService.UnlikeCommentAsync(userId, commentId);
                if (result)
                    return Ok(new { message = "Comment unliked successfully" });
                return BadRequest("Unable to unlike comment");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
