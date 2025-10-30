using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;

namespace SocialNetworkMobile.Services.Interfaces
{
    public interface IChatService
    {
        // Conversation methods
        Task<ConversationResponse> CreateConversationAsync(CreateConversationRequest request);
        Task<ConversationResponse?> GetConversationAsync(int user1Id, int user2Id);
        Task<ConversationResponse?> GetConversationByIdAsync(int conversationId);
        Task<List<ConversationResponse>> GetUserConversationsAsync(int userId);
        
        // Message methods
        Task<MessageResponse> SendMessageAsync(SendMessageRequest request);
        Task<List<MessageResponse>> GetConversationMessagesAsync(int conversationId, int page = 1, int limit = 50);
        
        // Group chat methods
        Task<GroupChatMessageResponse> SendGroupMessageAsync(SendGroupMessageRequest request);
        Task<List<GroupChatMessageResponse>> GetGroupMessagesAsync(int groupId, int page = 1, int limit = 50);
    }
}
