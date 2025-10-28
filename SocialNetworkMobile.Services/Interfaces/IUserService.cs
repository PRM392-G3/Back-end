using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialNetworkMobile.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResponse> CreateUserAsync(CreateUserRequest request);
        Task<UserResponse> GetUserByIdAsync(int id);
        Task<UserResponse> GetUserByEmailAsync(string email);
        Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request);
        Task<bool> DeleteUserAsync(int id);
        Task<UserResponse> LoginAsync(LoginRequest request);
        Task<bool> FollowUserAsync(int followerId, int followingId);
        Task<bool> UnfollowUserAsync(int followerId, int followingId);
        Task<bool> IsFollowingAsync(int followerId, int followingId);
        Task<bool> UpdateFcmTokenAsync(int userId, string fcmToken);
        Task<UserProfileResponse> GetUserProfileAsync(int userId, int? currentUserId = null);

        Task<List<UserResponse>> GetAllUsersAsync(int page = 1, int limit = 20);
        Task<List<UserResponse>> GetUserByNameAsync(string name, int page = 1, int limit = 20);
        Task<List<UserResponse>> GetFollowersAsync(int userId, int page = 1, int limit = 20);
        Task<List<UserResponse>> GetFollowingAsync(int userId, int page = 1, int limit = 20);
        Task<List<UserResponse>> GetFollowersWithStatusAsync(int userId, int currentUserId, int page = 1, int limit = 20);
        Task<List<UserResponse>> GetFollowingWithStatusAsync(int userId, int currentUserId, int page = 1, int limit = 20); 

        // Task<UserResponse> GoogleLoginAsync(GoogleLoginRequest request); 
    }
}