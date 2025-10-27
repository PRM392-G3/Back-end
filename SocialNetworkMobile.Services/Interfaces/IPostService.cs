using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;

namespace SocialNetworkMobile.Services.Interfaces
{
    public interface IPostService
    {
        Task<PostResponse> CreatePostAsync(CreatePostRequest request);
        Task<PostResponse> GetPostByIdAsync(int id);
        Task<List<PostResponse>> GetAllPostsAsync();
        Task<List<PostResponse>> GetAllPostsWithLikesAsync(int currentUserId);
        Task<List<PostResponse>> GetPostsByUserIdAsync(int userId);
        Task<List<PostResponse>> GetPostsByUserIdWithLikesAsync(int userId, int currentUserId);
        Task<List<PostResponse>> GetSharedPostsByUserIdAsync(int userId);
        Task<List<PostResponse>> GetSharedPostsByUserIdWithLikesAsync(int userId, int currentUserId);
        Task<List<PostResponse>> GetFeedPostsAsync(int userId, int page = 1, int pageSize = 10);
        Task<PostResponse> UpdatePostAsync(int id, UpdatePostRequest request);
        Task<bool> DeletePostAsync(int id);
        Task<bool> LikePostAsync(int userId, int postId);
        Task<bool> UnlikePostAsync(int userId, int postId);
        Task<List<UserResponse>> GetPostLikesAsync(int postId);
        Task<List<PostResponse>> SearchPostsAsync(string searchTerm);
        Task<List<PostResponse>> GetPostsByTagAsync(string tagName);
        Task<List<PostResponse>> GetPostsByGroupIdAsync(int groupId);
        Task<List<PostResponse>> GetPostsByGroupIdWithLikesAsync(int groupId, int currentUserId);
        
        /// <summary>
        /// ✅ TỐI ƯU N+1: Chỉ query METADATA (số like, comment, share)
        /// Comments được Lazy Load khi user bấm vào
        /// </summary>
        Task<List<PostFeedResponse>> GetOptimizedPostsFeedAsync(int currentUserId, int page = 1, int pageSize = 20);
    }
}
