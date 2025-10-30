using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;

namespace SocialNetworkMobile.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        // ==================== CONVERSATION ENDPOINTS ====================

        /// <summary>
        /// Create a new conversation between two users
        /// </summary>
        [HttpPost("conversations")]
        public async Task<ActionResult<ConversationResponse>> CreateConversation([FromBody] CreateConversationRequest request)
        {
            try
            {
                var conversation = await _chatService.CreateConversationAsync(request);
                return Ok(conversation);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get conversation between two users
        /// </summary>
        [HttpGet("conversations/{user1Id}/{user2Id}")]
        public async Task<ActionResult<ConversationResponse>> GetConversation(int user1Id, int user2Id)
        {
            try
            {
                var conversation = await _chatService.GetConversationAsync(user1Id, user2Id);
                if (conversation == null)
                    return NotFound("Conversation not found");
                
                return Ok(conversation);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get conversation by ID
        /// </summary>
        [HttpGet("conversations/{conversationId}")]
        public async Task<ActionResult<ConversationResponse>> GetConversationById(int conversationId)
        {
            try
            {
                var conversation = await _chatService.GetConversationByIdAsync(conversationId);
                if (conversation == null)
                    return NotFound("Conversation not found");
                
                return Ok(conversation);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get all conversations for a user
        /// </summary>
        [HttpGet("conversations/user/{userId}")]
        public async Task<ActionResult<List<ConversationResponse>>> GetUserConversations(int userId)
        {
            try
            {
                var conversations = await _chatService.GetUserConversationsAsync(userId);
                return Ok(conversations);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ==================== MESSAGE ENDPOINTS ====================

        /// <summary>
        /// Send a message in a conversation
        /// </summary>
        [HttpPost("messages")]
        public async Task<ActionResult<MessageResponse>> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                var message = await _chatService.SendMessageAsync(request);
                return Ok(message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get messages from a conversation
        /// </summary>
        [HttpGet("conversations/{conversationId}/messages")]
        public async Task<ActionResult<List<MessageResponse>>> GetConversationMessages(
            int conversationId, 
            [FromQuery] int page = 1, 
            [FromQuery] int limit = 50)
        {
            try
            {
                var messages = await _chatService.GetConversationMessagesAsync(conversationId, page, limit);
                return Ok(messages);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ==================== GROUP CHAT ENDPOINTS ====================

        /// <summary>
        /// Send a message in a group chat
        /// </summary>
        [HttpPost("group-messages")]
        public async Task<ActionResult<GroupChatMessageResponse>> SendGroupMessage([FromBody] SendGroupMessageRequest request)
        {
            try
            {
                var message = await _chatService.SendGroupMessageAsync(request);
                return Ok(message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get messages from a group chat
        /// </summary>
        [HttpGet("groups/{groupId}/messages")]
        public async Task<ActionResult<List<GroupChatMessageResponse>>> GetGroupMessages(
            int groupId, 
            [FromQuery] int page = 1, 
            [FromQuery] int limit = 50)
        {
            try
            {
                var messages = await _chatService.GetGroupMessagesAsync(groupId, page, limit);
                return Ok(messages);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ==================== UTILITY ENDPOINTS ====================

        /// <summary>
        /// Get or create conversation between two users
        /// </summary>
        [HttpPost("conversations/get-or-create")]
        public async Task<ActionResult<ConversationResponse>> GetOrCreateConversation([FromBody] CreateConversationRequest request)
        {
            try
            {
                // First try to get existing conversation
                var existingConversation = await _chatService.GetConversationAsync(request.User1Id, request.User2Id);
                if (existingConversation != null)
                    return Ok(existingConversation);

                // If not found, create new one
                var conversation = await _chatService.CreateConversationAsync(request);
                return Ok(conversation);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
