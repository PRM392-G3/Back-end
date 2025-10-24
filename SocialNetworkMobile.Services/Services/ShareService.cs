using Mapster;
using Microsoft.EntityFrameworkCore;
using SocialNetworkMobile.Repository.Context;
using SocialNetworkMobile.Repository.Models;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;

namespace SocialNetworkMobile.Services.Services
{
    public class ShareService : IShareService
    {
        private readonly SocialNetworkDbContext _context;

        public ShareService(SocialNetworkDbContext context)
        {
            _context = context;
        }

        public async Task<ShareResponse> SharePostAsync(SharePostRequest request)
        {
            // Check if post exists
            var post = await _context.Posts.FindAsync(request.PostId);
            if (post == null)
                throw new ArgumentException("Post not found");

            // Check if user exists
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                throw new ArgumentException("User not found");

            // Check if already shared
            var existingShare = await _context.Shares
                .FirstOrDefaultAsync(s => s.UserId == request.UserId && s.PostId == request.PostId);
            
            if (existingShare != null)
                throw new InvalidOperationException("Post already shared by this user");

            // Create new share
            var share = new Share
            {
                UserId = request.UserId,
                PostId = request.PostId,
                Caption = request.Caption,
                IsPublic = request.IsPublic,
                CreatedAt = DateTime.UtcNow
            };

            _context.Shares.Add(share);
            await _context.SaveChangesAsync();

            // Return response with user and post info
            var response = share.Adapt<ShareResponse>();
            response.User = user.Adapt<UserResponse>();
            response.Post = post.Adapt<PostResponse>();

            return response;
        }

        public async Task<bool> UnsharePostAsync(int userId, int postId)
        {
            var share = await _context.Shares
                .FirstOrDefaultAsync(s => s.UserId == userId && s.PostId == postId);
            
            if (share == null)
                return false;

            _context.Shares.Remove(share);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsPostSharedByUserAsync(int userId, int postId)
        {
            return await _context.Shares
                .AnyAsync(s => s.UserId == userId && s.PostId == postId);
        }

        public async Task<List<ShareResponse>> GetPostSharesAsync(int postId)
        {
            var shares = await _context.Shares
                .Include(s => s.User)
                .Include(s => s.Post)
                .Where(s => s.PostId == postId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return shares.Adapt<List<ShareResponse>>();
        }

        public async Task<List<ShareResponse>> GetUserSharesAsync(int userId)
        {
            var shares = await _context.Shares
                .Include(s => s.User)
                .Include(s => s.Post)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return shares.Adapt<List<ShareResponse>>();
        }

        public async Task<int> GetPostShareCountAsync(int postId)
        {
            return await _context.Shares.CountAsync(s => s.PostId == postId);
        }
    }
}