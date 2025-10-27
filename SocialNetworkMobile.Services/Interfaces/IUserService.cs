using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;

namespace SocialNetworkMobile.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResponse> CreateUserAsync(CreateUserRequest request);
        Task<UserResponse> GetUserByIdAsync(int id);
        Task<List<UserResponse>> GetUserByNameAsync(string name);
        Task<UserResponse> GetUserByEmailAsync(string email);
        Task<List<UserResponse>> GetAllUsersAsync();
        Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request);
        Task<bool> DeleteUserAsync(int id);
        Task<UserResponse> LoginAsync(LoginRequest request);
        Task<UserResponse> GoogleLoginAsync(GoogleLoginRequest request);
        Task<bool> FollowUserAsync(int followerId, int followingId);
        Task<bool> UnfollowUserAsync(int followerId, int followingId);
        Task<List<UserResponse>> GetFollowersAsync(int userId);
        Task<List<UserResponse>> GetFollowingAsync(int userId);
        Task<bool> IsFollowingAsync(int followerId, int followingId);
        Task<List<UserResponse>> GetFollowersWithStatusAsync(int userId, int currentUserId);
        Task<List<UserResponse>> GetFollowingWithStatusAsync(int userId, int currentUserId);
        Task<bool> UpdateFcmTokenAsync(int userId, string fcmToken);
        
        /// <summary>
        /// ✅ OPTIMIZED: Get lightweight user profile (fast loading)
        /// Only returns essential info, not full UserResponse
        /// </summary>
        Task<UserProfileResponse> GetUserProfileAsync(int userId, int? currentUserId = null);
    }
}
