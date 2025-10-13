using Microsoft.AspNetCore.Mvc;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;
using Microsoft.AspNetCore.Authorization;

namespace SocialNetworkMobile.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<PostResponse>> CreatePost([FromBody] CreatePostRequest request)
        {
            try
            {
                var post = await _postService.CreatePostAsync(request);
                return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PostResponse>> GetPost(int id)
        {
            try
            {
                var post = await _postService.GetPostByIdAsync(id);
                return Ok(post);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<PostResponse>>> GetAllPosts()
        {
            var posts = await _postService.GetAllPostsAsync();
            return Ok(posts);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<PostResponse>>> GetPostsByUser(int userId)
        {
            try
            {
                var posts = await _postService.GetPostsByUserIdAsync(userId);
                return Ok(posts);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("feed/{userId}")]
        [Authorize]
        public async Task<ActionResult<List<PostResponse>>> GetFeedPosts(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var posts = await _postService.GetFeedPostsAsync(userId, page, pageSize);
                return Ok(posts);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<PostResponse>> UpdatePost(int id, [FromBody] UpdatePostRequest request)
        {
            try
            {
                var post = await _postService.UpdatePostAsync(id, request);
                return Ok(post);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeletePost(int id)
        {
            try
            {
                var result = await _postService.DeletePostAsync(id);
                if (result)
                    return NoContent();
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{postId}/like/{userId}")]
        [Authorize]
        public async Task<ActionResult> LikePost(int postId, int userId)
        {
            try
            {
                var result = await _postService.LikePostAsync(userId, postId);
                if (result)
                    return Ok(new { message = "Post liked successfully" });
                return BadRequest("Unable to like post");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{postId}/like/{userId}")]
        [Authorize]
        public async Task<ActionResult> UnlikePost(int postId, int userId)
        {
            try
            {
                var result = await _postService.UnlikePostAsync(userId, postId);
                if (result)
                    return Ok(new { message = "Post unliked successfully" });
                return BadRequest("Unable to unlike post");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<PostResponse>>> SearchPosts([FromQuery] string searchTerm)
        {
            try
            {
                var posts = await _postService.SearchPostsAsync(searchTerm);
                return Ok(posts);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("tag/{tagName}")]
        public async Task<ActionResult<List<PostResponse>>> GetPostsByTag(string tagName)
        {
            try
            {
                var posts = await _postService.GetPostsByTagAsync(tagName);
                return Ok(posts);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
