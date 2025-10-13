using Microsoft.AspNetCore.Mvc;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;
using Microsoft.AspNetCore.Authorization;
using SocialNetworkMobile.Repository.Models;

namespace SocialNetworkMobile.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                var user = await _userService.CreateUserAsync(request);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponse>> GetUser(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<UserResponse>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<UserResponse>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = await _userService.UpdateUserAsync(id, request);
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);
                if (result)
                    return NoContent();
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{followerId}/follow/{followingId}")]
        [Authorize]
        public async Task<ActionResult> FollowUser(int followerId, int followingId)
        {
            try
            {
                var result = await _userService.FollowUserAsync(followerId, followingId);
                if (result)
                    return Ok(new { message = "User followed successfully" });
                return BadRequest("Unable to follow user");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{followerId}/follow/{followingId}")]
        [Authorize]
        public async Task<ActionResult> UnfollowUser(int followerId, int followingId)
        {
            try
            {
                var result = await _userService.UnfollowUserAsync(followerId, followingId);
                if (result)
                    return Ok(new { message = "User unfollowed successfully" });
                return BadRequest("Unable to unfollow user");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{userId}/followers")]
        public async Task<ActionResult<List<UserResponse>>> GetFollowers(int userId)
        {
            try
            {
                var followers = await _userService.GetFollowersAsync(userId);
                return Ok(followers);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{userId}/following")]
        public async Task<ActionResult<List<UserResponse>>> GetFollowing(int userId)
        {
            try
            {
                var following = await _userService.GetFollowingAsync(userId);
                return Ok(following);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{followerId}/is-following/{followingId}")]
        public async Task<ActionResult<bool>> IsFollowing(int followerId, int followingId)
        {
            try
            {
                var isFollowing = await _userService.IsFollowingAsync(followerId, followingId);
                return Ok(new { isFollowing });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{userId}/avatar-history")]
        [Authorize]
        public async Task<ActionResult<List<AvatarHistory>>> GetAvatarHistory(int userId)
        {
            try
            {
                var avatarHistory = await _userService.GetAvatarHistoryAsync(userId);
                return Ok(avatarHistory);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{userId}/restore-avatar/{avatarHistoryId}")]
        [Authorize]
        public async Task<ActionResult> RestoreAvatarFromHistory(int userId, int avatarHistoryId)
        {
            try
            {
                var result = await _userService.RestoreAvatarFromHistoryAsync(userId, avatarHistoryId);
                if (result)
                    return Ok(new { message = "Avatar restored successfully" });
                return BadRequest("Unable to restore avatar");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
