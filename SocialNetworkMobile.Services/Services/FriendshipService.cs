using Microsoft.EntityFrameworkCore;
using SocialNetworkMobile.Repository.Context;
using SocialNetworkMobile.Repository.Models;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;

namespace SocialNetworkMobile.Services.Services
{
    public class FriendshipService : IFriendshipService
    {
        private readonly SocialNetworkDbContext _context;

        public FriendshipService(SocialNetworkDbContext context)
        {
            _context = context;
        }

        public async Task<FriendshipResponse> SendFriendRequestAsync(SendFriendRequestRequest request)
        {
            // Validate users exist
            var requester = await _context.Users.FindAsync(request.RequesterId);
            var receiver = await _context.Users.FindAsync(request.ReceiverId);

            if (requester == null || receiver == null)
                throw new ArgumentException("One or both users not found");

            if (request.RequesterId == request.ReceiverId)
                throw new ArgumentException("Cannot send friend request to yourself");

            // Check if friendship already exists
            var existing = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.RequesterId == request.RequesterId && f.ReceiverId == request.ReceiverId) ||
                    (f.RequesterId == request.ReceiverId && f.ReceiverId == request.RequesterId));

            if (existing != null)
            {
                if (existing.Status == FriendshipStatus.Accepted)
                    throw new ArgumentException("Already friends");
                if (existing.Status == FriendshipStatus.Pending)
                    throw new ArgumentException("Friend request already pending");
                if (existing.Status == FriendshipStatus.Blocked)
                    throw new ArgumentException("Cannot send friend request");
            }

            var friendship = new Friendship
            {
                RequesterId = request.RequesterId,
                ReceiverId = request.ReceiverId,
                Status = FriendshipStatus.Pending,
                RequestedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Friendships.Add(friendship);
            await _context.SaveChangesAsync();

            return MapToResponse(friendship);
        }

        public async Task<FriendshipResponse> RespondToFriendRequestAsync(int friendshipId, RespondFriendRequestRequest request)
        {
            var friendship = await _context.Friendships
                .AsTracking()  // Enable tracking cho query này
                .Include(f => f.Requester)
                .Include(f => f.Receiver)
                .FirstOrDefaultAsync(f => f.Id == friendshipId);

            if (friendship == null)
                throw new ArgumentException("Friend request not found");

            if (friendship.Status != FriendshipStatus.Pending)
                throw new ArgumentException("Friend request is not pending");

            friendship.Status = request.Status;
            friendship.RespondedAt = DateTime.UtcNow;
            friendship.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponse(friendship);
        }

        public async Task<bool> CancelFriendRequestAsync(int friendshipId, int requesterId)
        {
            var friendship = await _context.Friendships
                .AsTracking()  // Enable tracking
                .FirstOrDefaultAsync(f => f.Id == friendshipId && f.RequesterId == requesterId);

            if (friendship == null)
                return false;

            if (friendship.Status != FriendshipStatus.Pending)
                throw new ArgumentException("Can only cancel pending requests");

            _context.Friendships.Remove(friendship);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UnfriendAsync(int userId1, int userId2)
        {
            var friendship = await _context.Friendships
                .AsTracking()  // Enable tracking
                .FirstOrDefaultAsync(f =>
                    f.Status == FriendshipStatus.Accepted &&
                    ((f.RequesterId == userId1 && f.ReceiverId == userId2) ||
                     (f.RequesterId == userId2 && f.ReceiverId == userId1)));

            if (friendship == null)
                return false;

            _context.Friendships.Remove(friendship);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> BlockUserAsync(int blockerId, int blockedId)
        {
            if (blockerId == blockedId)
                throw new ArgumentException("Cannot block yourself");

            // Remove existing friendship if any
            var existing = await _context.Friendships
                .AsTracking()  // Enable tracking
                .FirstOrDefaultAsync(f =>
                    (f.RequesterId == blockerId && f.ReceiverId == blockedId) ||
                    (f.RequesterId == blockedId && f.ReceiverId == blockerId));

            if (existing != null)
            {
                _context.Friendships.Remove(existing);
            }

            // Create block record
            var block = new Friendship
            {
                RequesterId = blockerId,
                ReceiverId = blockedId,
                Status = FriendshipStatus.Blocked,
                RequestedAt = DateTime.UtcNow,
                RespondedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Friendships.Add(block);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UnblockUserAsync(int blockerId, int blockedId)
        {
            var block = await _context.Friendships
                .AsTracking()  // Enable tracking
                .FirstOrDefaultAsync(f =>
                    f.RequesterId == blockerId &&
                    f.ReceiverId == blockedId &&
                    f.Status == FriendshipStatus.Blocked);

            if (block == null)
                return false;

            _context.Friendships.Remove(block);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<UserResponse>> GetFriendsAsync(int userId)
        {
            var friendships = await _context.Friendships
                .Where(f => f.Status == FriendshipStatus.Accepted &&
                           (f.RequesterId == userId || f.ReceiverId == userId))
                .Include(f => f.Requester)
                .Include(f => f.Receiver)
                .ToListAsync();

            var friends = friendships.Select(f =>
            {
                var friend = f.RequesterId == userId ? f.Receiver : f.Requester;
                return MapUserToResponse(friend);
            }).ToList();

            return friends;
        }

        public async Task<List<FriendshipResponse>> GetPendingRequestsAsync(int userId)
        {
            var requests = await _context.Friendships
                .Where(f => f.ReceiverId == userId && f.Status == FriendshipStatus.Pending)
                .Include(f => f.Requester)
                .Include(f => f.Receiver)
                .OrderByDescending(f => f.RequestedAt)
                .ToListAsync();

            return requests.Select(MapToResponse).ToList();
        }

        public async Task<List<FriendshipResponse>> GetSentRequestsAsync(int userId)
        {
            var requests = await _context.Friendships
                .Where(f => f.RequesterId == userId && f.Status == FriendshipStatus.Pending)
                .Include(f => f.Requester)
                .Include(f => f.Receiver)
                .OrderByDescending(f => f.RequestedAt)
                .ToListAsync();

            return requests.Select(MapToResponse).ToList();
        }

        public async Task<bool> AreFriendsAsync(int userId1, int userId2)
        {
            return await _context.Friendships
                .AnyAsync(f =>
                    f.Status == FriendshipStatus.Accepted &&
                    ((f.RequesterId == userId1 && f.ReceiverId == userId2) ||
                     (f.RequesterId == userId2 && f.ReceiverId == userId1)));
        }

        public async Task<string?> GetFriendshipStatusAsync(int userId1, int userId2)
        {
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.RequesterId == userId1 && f.ReceiverId == userId2) ||
                    (f.RequesterId == userId2 && f.ReceiverId == userId1));

            return friendship?.Status;
        }

        public async Task<List<UserResponse>> GetMutualFriendsAsync(int userId1, int userId2)
        {
            var user1Friends = await _context.Friendships
                .Where(f => f.Status == FriendshipStatus.Accepted &&
                           (f.RequesterId == userId1 || f.ReceiverId == userId1))
                .Select(f => f.RequesterId == userId1 ? f.ReceiverId : f.RequesterId)
                .ToListAsync();

            var user2Friends = await _context.Friendships
                .Where(f => f.Status == FriendshipStatus.Accepted &&
                           (f.RequesterId == userId2 || f.ReceiverId == userId2))
                .Select(f => f.RequesterId == userId2 ? f.ReceiverId : f.RequesterId)
                .ToListAsync();

            var mutualFriendIds = user1Friends.Intersect(user2Friends).ToList();

            var mutualFriends = await _context.Users
                .Where(u => mutualFriendIds.Contains(u.Id))
                .ToListAsync();

            return mutualFriends.Select(MapUserToResponse).ToList();
        }

        public async Task<List<UserResponse>> GetFriendSuggestionsAsync(int userId, int limit = 10)
        {
            // Get user's friends
            var userFriendIds = await _context.Friendships
                .Where(f => f.Status == FriendshipStatus.Accepted &&
                           (f.RequesterId == userId || f.ReceiverId == userId))
                .Select(f => f.RequesterId == userId ? f.ReceiverId : f.RequesterId)
                .ToListAsync();

            // Get friends of friends
            var friendsOfFriends = await _context.Friendships
                .Where(f => f.Status == FriendshipStatus.Accepted &&
                           userFriendIds.Contains(f.RequesterId) || userFriendIds.Contains(f.ReceiverId))
                .Select(f => userFriendIds.Contains(f.RequesterId) ? f.ReceiverId : f.RequesterId)
                .Where(id => id != userId && !userFriendIds.Contains(id))
                .Distinct()
                .Take(limit)
                .ToListAsync();

            var suggestions = await _context.Users
                .Where(u => friendsOfFriends.Contains(u.Id))
                .ToListAsync();

            return suggestions.Select(MapUserToResponse).ToList();
        }

        // Helper methods
        private FriendshipResponse MapToResponse(Friendship friendship)
        {
            return new FriendshipResponse
            {
                Id = friendship.Id,
                RequesterId = friendship.RequesterId,
                ReceiverId = friendship.ReceiverId,
                Status = friendship.Status,
                RequestedAt = friendship.RequestedAt,
                RespondedAt = friendship.RespondedAt,
                Requester = friendship.Requester != null ? MapUserToResponse(friendship.Requester) : null,
                Receiver = friendship.Receiver != null ? MapUserToResponse(friendship.Receiver) : null
            };
        }

        private UserResponse MapUserToResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio,
                Location = user.Location,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}

