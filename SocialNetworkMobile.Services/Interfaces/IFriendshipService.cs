using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;

namespace SocialNetworkMobile.Services.Interfaces
{
    public interface IFriendshipService
    {
        // Send friend request
        Task<FriendshipResponse> SendFriendRequestAsync(SendFriendRequestRequest request);

        // Accept or reject friend request
        Task<FriendshipResponse> RespondToFriendRequestAsync(int friendshipId, RespondFriendRequestRequest request);

        // Cancel friend request (by requester)
        Task<bool> CancelFriendRequestAsync(int friendshipId, int requesterId);

        // Unfriend
        Task<bool> UnfriendAsync(int userId1, int userId2);

        // Block user
        Task<bool> BlockUserAsync(int blockerId, int blockedId);

        // Unblock user
        Task<bool> UnblockUserAsync(int blockerId, int blockedId);

        // Get all friends of a user
        Task<List<UserResponse>> GetFriendsAsync(int userId);

        // Get pending friend requests (received)
        Task<List<FriendshipResponse>> GetPendingRequestsAsync(int userId);

        // Get sent friend requests
        Task<List<FriendshipResponse>> GetSentRequestsAsync(int userId);

        // Check if two users are friends
        Task<bool> AreFriendsAsync(int userId1, int userId2);

        // Get friendship status between two users
        Task<string?> GetFriendshipStatusAsync(int userId1, int userId2);

        // Get mutual friends
        Task<List<UserResponse>> GetMutualFriendsAsync(int userId1, int userId2);

        // Get friend suggestions (friends of friends)
        Task<List<UserResponse>> GetFriendSuggestionsAsync(int userId, int limit = 10);
    }
}

