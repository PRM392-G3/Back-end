using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;

namespace SocialNetworkMobile.Services.Interfaces
{
    public interface IShareService
    {
        Task<ShareResponse> SharePostAsync(SharePostRequest request);
        Task<bool> UnsharePostAsync(int userId, int postId);
        Task<List<ShareResponse>> GetSharesByPostAsync(int postId);
        Task<List<ShareResponse>> GetSharesByUserAsync(int userId);
        Task<bool> HasUserSharedPostAsync(int userId, int postId);
        Task<int> GetShareCountAsync(int postId);
    }
}
