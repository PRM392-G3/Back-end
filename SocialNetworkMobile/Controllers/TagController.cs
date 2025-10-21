using Microsoft.AspNetCore.Mvc;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;
using Microsoft.AspNetCore.Authorization;

namespace SocialNetworkMobile.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagController(ITagService tagService)
        {
            _tagService = tagService;
        }

        /// <summary>
        /// Get all available tags
        /// </summary>
        /// <returns>List of all tags</returns>
        [HttpGet]
        public async Task<ActionResult<List<TagResponse>>> GetAllTags()
        {
            try
            {
                var tags = await _tagService.GetAllTagsAsync();
                return Ok(tags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving tags", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific tag by ID
        /// </summary>
        /// <param name="id">Tag ID</param>
        /// <returns>Tag details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<TagResponse>> GetTag(int id)
        {
            try
            {
                var tag = await _tagService.GetTagByIdAsync(id);
                return Ok(tag);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving tag", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new tag
        /// </summary>
        /// <param name="request">Tag creation request</param>
        /// <returns>Created tag</returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<TagResponse>> CreateTag([FromBody] CreateTagRequest request)
        {
            try
            {
                var tag = await _tagService.CreateTagAsync(request);
                return CreatedAtAction(nameof(GetTag), new { id = tag.Id }, tag);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating tag", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing tag
        /// </summary>
        /// <param name="id">Tag ID</param>
        /// <param name="request">Tag update request</param>
        /// <returns>Updated tag</returns>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<TagResponse>> UpdateTag(int id, [FromBody] UpdateTagRequest request)
        {
            try
            {
                var tag = await _tagService.UpdateTagAsync(id, request);
                return Ok(tag);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating tag", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a tag
        /// </summary>
        /// <param name="id">Tag ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteTag(int id)
        {
            try
            {
                var result = await _tagService.DeleteTagAsync(id);
                if (result)
                    return NoContent();
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting tag", error = ex.Message });
            }
        }

        /// <summary>
        /// Search tags by name
        /// </summary>
        /// <param name="term">Search term</param>
        /// <returns>Matching tags</returns>
        [HttpGet("search")]
        public async Task<ActionResult<List<TagResponse>>> SearchTags([FromQuery] string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                {
                    return BadRequest(new { message = "Search term is required" });
                }

                var tags = await _tagService.SearchTagsAsync(term);
                return Ok(tags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error searching tags", error = ex.Message });
            }
        }

        /// <summary>
        /// Get popular tags (most used)
        /// </summary>
        /// <param name="limit">Number of tags to return (default: 10)</param>
        /// <returns>Popular tags</returns>
        [HttpGet("popular")]
        public async Task<ActionResult<List<TagResponse>>> GetPopularTags([FromQuery] int limit = 10)
        {
            try
            {
                var tags = await _tagService.GetPopularTagsAsync(limit);
                return Ok(tags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving popular tags", error = ex.Message });
            }
        }
    }
}