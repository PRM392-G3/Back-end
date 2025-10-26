using Microsoft.EntityFrameworkCore;
using SocialNetworkMobile.Repository.Context;
using SocialNetworkMobile.Repository.Models;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;

namespace SocialNetworkMobile.Services.Services
{
    public class ChatService : IChatService
    {
        private readonly SocialNetworkDbContext _context;

        public ChatService(SocialNetworkDbContext context)
        {
            _context = context;
        }

        public async Task<ConversationResponse> CreateConversationAsync(CreateConversationRequest request)
        {
            // Check if conversation already exists
            var existingConversation = await _context.Conversations
                .FirstOrDefaultAsync(c => 
                    (c.User1Id == request.User1Id && c.User2Id == request.User2Id) ||
                    (c.User1Id == request.User2Id && c.User2Id == request.User1Id));

            if (existingConversation != null)
            {
                return await GetConversationResponseAsync(existingConversation);
            }

            var conversation = new Conversation
            {
                User1Id = request.User1Id,
                User2Id = request.User2Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            return await GetConversationResponseAsync(conversation);
        }

        public async Task<ConversationResponse?> GetConversationAsync(int user1Id, int user2Id)
        {
            var conversation = await _context.Conversations
                .Include(c => c.User1)
                .Include(c => c.User2)
                .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .FirstOrDefaultAsync(c => 
                    (c.User1Id == user1Id && c.User2Id == user2Id) ||
                    (c.User1Id == user2Id && c.User2Id == user1Id));

            if (conversation == null)
                return null;

            return await GetConversationResponseAsync(conversation);
        }

        public async Task<List<ConversationResponse>> GetUserConversationsAsync(int userId)
        {
            var conversations = await _context.Conversations
                .Include(c => c.User1)
                .Include(c => c.User2)
                .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .Where(c => c.User1Id == userId || c.User2Id == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var result = new List<ConversationResponse>();
            foreach (var conversation in conversations)
            {
                result.Add(await GetConversationResponseAsync(conversation));
            }

            return result;
        }

        public async Task<MessageResponse> SendMessageAsync(SendMessageRequest request)
        {
            var message = new Message
            {
                ConversationId = request.ConversationId,
                SenderId = request.SenderId,
                Content = request.Content,
                ImageUrl = request.ImageUrl,
                VideoUrl = request.VideoUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return await GetMessageResponseAsync(message);
        }

        public async Task<List<MessageResponse>> GetConversationMessagesAsync(int conversationId, int page = 1, int limit = 50)
        {
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            var result = new List<MessageResponse>();
            foreach (var message in messages.OrderBy(m => m.CreatedAt))
            {
                result.Add(await GetMessageResponseAsync(message));
            }

            return result;
        }

        public async Task<GroupChatMessageResponse> SendGroupMessageAsync(SendGroupMessageRequest request)
        {
            var message = new GroupChatMessage
            {
                GroupId = request.GroupId,
                SenderId = request.SenderId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.GroupChatMessages.Add(message);
            await _context.SaveChangesAsync();

            return await GetGroupChatMessageResponseAsync(message);
        }

        public async Task<List<GroupChatMessageResponse>> GetGroupMessagesAsync(int groupId, int page = 1, int limit = 50)
        {
            var messages = await _context.GroupChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Group)
                .Where(m => m.GroupId == groupId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            var result = new List<GroupChatMessageResponse>();
            foreach (var message in messages.OrderBy(m => m.CreatedAt))
            {
                result.Add(await GetGroupChatMessageResponseAsync(message));
            }

            return result;
        }

        private async Task<ConversationResponse> GetConversationResponseAsync(Conversation conversation)
        {
            var lastMessage = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ConversationId == conversation.Id)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();

            return new ConversationResponse
            {
                Id = conversation.Id,
                User1Id = conversation.User1Id,
                User2Id = conversation.User2Id,
                User1Name = conversation.User1?.FullName ?? "Unknown",
                User2Name = conversation.User2?.FullName ?? "Unknown",
                User1AvatarUrl = conversation.User1?.AvatarUrl ?? "",
                User2AvatarUrl = conversation.User2?.AvatarUrl ?? "",
                CreatedAt = conversation.CreatedAt,
                LastMessage = lastMessage != null ? await GetMessageResponseAsync(lastMessage) : null
            };
        }

        private async Task<MessageResponse> GetMessageResponseAsync(Message message)
        {
            return new MessageResponse
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderId = message.SenderId,
                SenderName = message.Sender?.FullName ?? "Unknown",
                SenderAvatarUrl = message.Sender?.AvatarUrl ?? "",
                Content = message.Content ?? "",
                ImageUrl = message.ImageUrl,
                VideoUrl = message.VideoUrl,
                CreatedAt = message.CreatedAt
            };
        }

        private async Task<GroupChatMessageResponse> GetGroupChatMessageResponseAsync(GroupChatMessage message)
        {
            return new GroupChatMessageResponse
            {
                Id = message.Id,
                GroupId = message.GroupId,
                GroupName = message.Group?.Name ?? "Unknown",
                SenderId = message.SenderId,
                SenderName = message.Sender?.FullName ?? "Unknown",
                SenderAvatarUrl = message.Sender?.AvatarUrl ?? "",
                Content = message.Content ?? "",
                CreatedAt = message.CreatedAt
            };
        }
    }
}
