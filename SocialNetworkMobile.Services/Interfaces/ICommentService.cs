using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;

namespace SocialNetworkMobile.Services.Interfaces
{
    public interface ICommentService
    {
        Task<CommentResponse> CreateCommentAsync(CreateCommentRequest request);
        Task<CommentResponse> GetCommentByIdAsync(int id);
        Task<List<CommentResponse>> GetCommentsByPostIdAsync(int postId);
        Task<List<CommentResponse>> GetCommentsByReelIdAsync(int reelId);
        Task<List<CommentResponse>> GetRepliesByCommentIdAsync(int commentId);
        Task<CommentResponse> UpdateCommentAsync(int id, UpdateCommentRequest request);
        Task<bool> DeleteCommentAsync(int id);
        Task<bool> LikeCommentAsync(int userId, int commentId);
        Task<bool> UnlikeCommentAsync(int userId, int commentId);
    }
}
