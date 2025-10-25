using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;

namespace SocialNetworkMobile.Services.Interfaces
{
    public interface IReelService
    {
        Task<ReelResponse> CreateReelAsync(CreateReelRequest request);
        Task<ReelResponse> GetReelByIdAsync(int id);
        Task<List<ReelResponse>> GetAllReelsAsync(int? currentUserId = null);
        Task<List<ReelResponse>> GetReelsByUserIdAsync(int userId);
        Task<bool> DeleteReelAsync(int id, int userId);
        
        // Like/Unlike
        Task<bool> LikeReelAsync(int userId, int reelId);
        Task<bool> UnlikeReelAsync(int userId, int reelId);
        
        // Comment management
        Task<bool> UpdateCommentCountAsync(int reelId);
        
        // Music management
        Task<ReelMusicResponse> CreateMusicAsync(CreateReelMusicRequest request);
        Task<List<ReelMusicResponse>> GetAllMusicAsync();
        Task<ReelMusicResponse> GetMusicByIdAsync(int id);
    }
}
