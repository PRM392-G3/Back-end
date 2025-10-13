using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;

namespace SocialNetworkMobile.Services.Interfaces
{
    public interface IPostService
    {
        Task<PostResponse> CreatePostAsync(CreatePostRequest request);
        Task<PostResponse> GetPostByIdAsync(int id);
        Task<List<PostResponse>> GetAllPostsAsync();
        Task<List<PostResponse>> GetPostsByUserIdAsync(int userId);
        Task<List<PostResponse>> GetFeedPostsAsync(int userId, int page = 1, int pageSize = 10);
        Task<PostResponse> UpdatePostAsync(int id, UpdatePostRequest request);
        Task<bool> DeletePostAsync(int id);
        Task<bool> LikePostAsync(int userId, int postId);
        Task<bool> UnlikePostAsync(int userId, int postId);
        Task<List<PostResponse>> SearchPostsAsync(string searchTerm);
        Task<List<PostResponse>> GetPostsByTagAsync(string tagName);
    }
}
