using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;

namespace SocialNetworkMobile.Services.Interfaces
{
    public interface IShareService
    {
        Task<ShareResponse> SharePostAsync(SharePostRequest request);
        Task<bool> UnsharePostAsync(int userId, int postId);
        Task<bool> IsPostSharedByUserAsync(int userId, int postId);
        Task<List<ShareResponse>> GetPostSharesAsync(int postId);
        Task<List<ShareResponse>> GetUserSharesAsync(int userId);
        Task<int> GetPostShareCountAsync(int postId);
    }
}