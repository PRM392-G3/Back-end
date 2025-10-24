using Microsoft.AspNetCore.Mvc;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;
using Microsoft.AspNetCore.Authorization;

namespace SocialNetworkMobile.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IFriendshipService _friendshipService;

        public UserController(IUserService userService, IFriendshipService friendshipService)
        {
            _userService = userService;
            _friendshipService = friendshipService;
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

        /// <summary>
        /// Search users by name (case-insensitive)
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<List<UserResponse>>> SearchUsersByName([FromQuery] string name)
        {
            try
            {
                var users = await _userService.GetUserByNameAsync(name);

                if (users == null)
                    return NotFound(new { message = "User not found" });

                return Ok(users);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred", details = ex.Message });
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

        [HttpGet("{userId}/followers/with-status")]
        [Authorize]
        public async Task<ActionResult<List<UserResponse>>> GetFollowersWithStatus(int userId)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                if (currentUserId == 0)
                    return Unauthorized(new { error = "Invalid user token" });

                var followers = await _userService.GetFollowersWithStatusAsync(userId, currentUserId);
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

        [HttpGet("{userId}/following/with-status")]
        [Authorize]
        public async Task<ActionResult<List<UserResponse>>> GetFollowingWithStatus(int userId)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                if (currentUserId == 0)
                    return Unauthorized(new { error = "Invalid user token" });

                var following = await _userService.GetFollowingWithStatusAsync(userId, currentUserId);
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

        // ==================== FRIENDSHIP ENDPOINTS ====================

        /// <summary>
        /// Send a friend request
        /// </summary>
        [HttpPost("friend-request")]
        [Authorize]
        public async Task<ActionResult<FriendshipResponse>> SendFriendRequest([FromBody] SendFriendRequestRequest request)
        {
            try
            {
                var result = await _friendshipService.SendFriendRequestAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Accept or reject a friend request
        /// </summary>
        [HttpPut("friend-request/{friendshipId}/respond")]
        [Authorize]
        public async Task<ActionResult<FriendshipResponse>> RespondToFriendRequest(int friendshipId, [FromBody] RespondFriendRequestRequest request)
        {
            try
            {
                var result = await _friendshipService.RespondToFriendRequestAsync(friendshipId, request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Cancel a pending friend request
        /// </summary>
        [HttpDelete("friend-request/{friendshipId}/cancel")]
        [Authorize]
        public async Task<ActionResult> CancelFriendRequest(int friendshipId, [FromQuery] int requesterId)
        {
            try
            {
                var result = await _friendshipService.CancelFriendRequestAsync(friendshipId, requesterId);
                if (result)
                    return Ok(new { message = "Friend request cancelled" });
                return NotFound("Friend request not found");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Unfriend a user
        /// </summary>
        [HttpDelete("{userId1}/unfriend/{userId2}")]
        [Authorize]
        public async Task<ActionResult> Unfriend(int userId1, int userId2)
        {
            try
            {
                var result = await _friendshipService.UnfriendAsync(userId1, userId2);
                if (result)
                    return Ok(new { message = "Successfully unfriended" });
                return NotFound("Friendship not found");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Block a user
        /// </summary>
        [HttpPost("{blockerId}/block/{blockedId}")]
        [Authorize]
        public async Task<ActionResult> BlockUser(int blockerId, int blockedId)
        {
            try
            {
                var result = await _friendshipService.BlockUserAsync(blockerId, blockedId);
                if (result)
                    return Ok(new { message = "User blocked successfully" });
                return BadRequest("Unable to block user");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Unblock a user
        /// </summary>
        [HttpDelete("{blockerId}/unblock/{blockedId}")]
        [Authorize]
        public async Task<ActionResult> UnblockUser(int blockerId, int blockedId)
        {
            try
            {
                var result = await _friendshipService.UnblockUserAsync(blockerId, blockedId);
                if (result)
                    return Ok(new { message = "User unblocked successfully" });
                return NotFound("Block not found");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get all friends of a user
        /// </summary>
        [HttpGet("{userId}/friends")]
        public async Task<ActionResult<List<UserResponse>>> GetFriends(int userId)
        {
            try
            {
                var friends = await _friendshipService.GetFriendsAsync(userId);
                return Ok(friends);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get pending friend requests (received)
        /// </summary>
        [HttpGet("{userId}/friend-requests/pending")]
        [Authorize]
        public async Task<ActionResult<List<FriendshipResponse>>> GetPendingRequests(int userId)
        {
            try
            {
                var requests = await _friendshipService.GetPendingRequestsAsync(userId);
                return Ok(requests);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get sent friend requests
        /// </summary>
        [HttpGet("{userId}/friend-requests/sent")]
        [Authorize]
        public async Task<ActionResult<List<FriendshipResponse>>> GetSentRequests(int userId)
        {
            try
            {
                var requests = await _friendshipService.GetSentRequestsAsync(userId);
                return Ok(requests);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Check if two users are friends
        /// </summary>
        [HttpGet("{userId1}/are-friends/{userId2}")]
        public async Task<ActionResult<bool>> AreFriends(int userId1, int userId2)
        {
            try
            {
                var areFriends = await _friendshipService.AreFriendsAsync(userId1, userId2);
                return Ok(new { areFriends });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get friendship status between two users
        /// </summary>
        [HttpGet("{userId1}/friendship-status/{userId2}")]
        public async Task<ActionResult> GetFriendshipStatus(int userId1, int userId2)
        {
            try
            {
                var status = await _friendshipService.GetFriendshipStatusAsync(userId1, userId2);
                return Ok(new { status = status ?? "none" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get mutual friends between two users
        /// </summary>
        [HttpGet("{userId1}/mutual-friends/{userId2}")]
        public async Task<ActionResult<List<UserResponse>>> GetMutualFriends(int userId1, int userId2)
        {
            try
            {
                var mutualFriends = await _friendshipService.GetMutualFriendsAsync(userId1, userId2);
                return Ok(mutualFriends);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get friend suggestions (friends of friends)
        /// </summary>
        [HttpGet("{userId}/friend-suggestions")]
        public async Task<ActionResult<List<UserResponse>>> GetFriendSuggestions(int userId, [FromQuery] int limit = 10)
        {
            try
            {
                var suggestions = await _friendshipService.GetFriendSuggestionsAsync(userId, limit);
                return Ok(suggestions);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
