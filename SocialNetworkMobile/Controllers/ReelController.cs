using Microsoft.AspNetCore.Mvc;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;
using Microsoft.AspNetCore.Authorization;

namespace SocialNetworkMobile.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReelController : ControllerBase
    {
        private readonly IReelService _reelService;

        public ReelController(IReelService reelService)
        {
            _reelService = reelService;
        }

        /// <summary>
        /// Create a new reel
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReelResponse>> CreateReel([FromBody] CreateReelRequest request)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                if (currentUserId == 0)
                    return Unauthorized(new { error = "Invalid user token" });

                // Override userId with current user
                request.UserId = currentUserId;

                var reel = await _reelService.CreateReelAsync(request);
                return CreatedAtAction(nameof(GetReel), new { id = reel.Id }, reel);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get a reel by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ReelResponse>> GetReel(int id)
        {
            try
            {
                var reel = await _reelService.GetReelByIdAsync(id);
                return Ok(reel);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Get all public reels
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ReelResponse>>> GetAllReels()
        {
            try
            {
                var reels = await _reelService.GetAllReelsAsync();
                return Ok(reels);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving reels", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all reels by a specific user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<ReelResponse>>> GetReelsByUser(int userId)
        {
            try
            {
                var reels = await _reelService.GetReelsByUserIdAsync(userId);
                return Ok(reels);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete a reel
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteReel(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                if (currentUserId == 0)
                    return Unauthorized(new { error = "Invalid user token" });

                var result = await _reelService.DeleteReelAsync(id, currentUserId);
                if (result)
                    return NoContent();
                return NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Music management endpoints

        /// <summary>
        /// Create a new music for reels
        /// </summary>
        [HttpPost("music")]
        [Authorize]
        public async Task<ActionResult<ReelMusicResponse>> CreateMusic([FromBody] CreateReelMusicRequest request)
        {
            try
            {
                var music = await _reelService.CreateMusicAsync(request);
                return CreatedAtAction(nameof(GetMusic), new { id = music.Id }, music);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get all available music
        /// </summary>
        [HttpGet("music")]
        public async Task<ActionResult<List<ReelMusicResponse>>> GetAllMusic()
        {
            try
            {
                var musics = await _reelService.GetAllMusicAsync();
                return Ok(musics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving music", error = ex.Message });
            }
        }

        /// <summary>
        /// Get music by ID
        /// </summary>
        [HttpGet("music/{id}")]
        public async Task<ActionResult<ReelMusicResponse>> GetMusic(int id)
        {
            try
            {
                var music = await _reelService.GetMusicByIdAsync(id);
                return Ok(music);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
