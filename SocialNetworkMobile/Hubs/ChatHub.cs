using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;

namespace SocialNetworkMobile.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        // User connection tracking
        private static readonly Dictionary<string, string> ConnectedUsers = new Dictionary<string, string>();
        
        private readonly IChatService _chatService;
        private readonly INotificationService _notificationService;

        public ChatHub(IChatService chatService, INotificationService notificationService)
        {
            _chatService = chatService;
            _notificationService = notificationService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.Identity?.Name;
            if (userId != null)
            {
                ConnectedUsers[Context.ConnectionId] = userId;
                await Clients.All.SendAsync("UserConnected", userId);
                Console.WriteLine($"[ChatHub] User {userId} connected");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.Identity?.Name;
            if (userId != null && ConnectedUsers.ContainsKey(Context.ConnectionId))
            {
                ConnectedUsers.Remove(Context.ConnectionId);
                await Clients.All.SendAsync("UserDisconnected", userId);
                Console.WriteLine($"[ChatHub] User {userId} disconnected");
            }
            await base.OnDisconnectedAsync(exception);
        }

        // Send message to specific user
        public async Task SendMessageToUser(string toUserId, string content, int conversationId)
        {
            var fromUserId = Context.User?.Identity?.Name;
            if (fromUserId == null) return;

            try
            {
                // Save message to database
                var messageRequest = new SendMessageRequest
                {
                    ConversationId = conversationId,
                    SenderId = int.Parse(fromUserId),
                    Content = content,
                    ImageUrl = null,
                    VideoUrl = null
                };

                var savedMessage = await _chatService.SendMessageAsync(messageRequest);

                // Find user's connection(s)
                var targetConnections = ConnectedUsers
                    .Where(x => x.Value == toUserId)
                    .Select(x => x.Key)
                    .ToList();

                // Send to recipient if online
                if (targetConnections.Any())
                {
                    await Clients.Clients(targetConnections).SendAsync("ReceiveMessage", savedMessage);
                }

                // Notify sender that message was sent
                await Clients.Caller.SendAsync("MessageSent", savedMessage);

                Console.WriteLine($"[ChatHub] Message sent from {fromUserId} to {toUserId}");
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorMessage", $"Error sending message: {ex.Message}");
                Console.WriteLine($"[ChatHub] Error sending message: {ex.Message}");
            }
        }

        // Send message to group
        public async Task SendMessageToGroup(int groupId, string message)
        {
            var userId = Context.User?.Identity?.Name;
            if (userId == null) return;

            await Clients.Group($"group_{groupId}").SendAsync("ReceiveGroupMessage", new
            {
                userId,
                message,
                groupId,
                timestamp = DateTime.UtcNow
            });

            Console.WriteLine($"[ChatHub] Group message sent to group {groupId} by {userId}");
        }

        // Join a group chat
        public async Task JoinGroupChat(int groupId)
        {
            var userId = Context.User?.Identity?.Name;
            if (userId == null) return;

            await Groups.AddToGroupAsync(Context.ConnectionId, $"group_{groupId}");
            await Clients.Group($"group_{groupId}").SendAsync("UserJoinedGroup", new
            {
                userId,
                groupId
            });

            Console.WriteLine($"[ChatHub] User {userId} joined group {groupId}");
        }

        // Leave a group chat
        public async Task LeaveGroupChat(int groupId)
        {
            var userId = Context.User?.Identity?.Name;
            if (userId == null) return;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group_{groupId}");
            await Clients.Group($"group_{groupId}").SendAsync("UserLeftGroup", new
            {
                userId,
                groupId
            });

            Console.WriteLine($"[ChatHub] User {userId} left group {groupId}");
        }

        // Typing indicator
        public async Task SendTypingIndicator(string toUserId, bool isTyping)
        {
            var fromUserId = Context.User?.Identity?.Name;
            if (fromUserId == null) return;

            var targetConnections = ConnectedUsers
                .Where(x => x.Value == toUserId)
                .Select(x => x.Key)
                .ToList();

            if (targetConnections.Any())
            {
                await Clients.Clients(targetConnections).SendAsync("UserTyping", new
                {
                    userId = fromUserId,
                    isTyping
                });
            }
        }
    }
}

