using SocialNetworkMobile.Repository.Basic;
using SocialNetworkMobile.Repository.Models;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;
using Mapster;

namespace SocialNetworkMobile.Services.Services
{
    public class ShareService : IShareService
    {
        private readonly GenericRepository<Share> _shareRepository;
        private readonly GenericRepository<Post> _postRepository;
        private readonly GenericRepository<User> _userRepository;

        public ShareService(
            GenericRepository<Share> shareRepository,
            GenericRepository<Post> postRepository,
            GenericRepository<User> userRepository)
        {
            _shareRepository = shareRepository;
            _postRepository = postRepository;
            _userRepository = userRepository;
        }

        public async Task<ShareResponse> SharePostAsync(SharePostRequest request)
        {
            // Check if user has already shared this post
            var existingShare = await _shareRepository.GetFirstOrDefaultAsync(
                s => s.UserId == request.UserId && s.PostId == request.PostId);

            if (existingShare != null)
            {
                throw new InvalidOperationException("User has already shared this post");
            }

            // Verify post exists
            var post = await _postRepository.GetByIdAsync(request.PostId);
            if (post == null)
            {
                throw new ArgumentException("Post not found");
            }

            // Create new share
            var share = new Share
            {
                UserId = request.UserId,
                PostId = request.PostId,
                Caption = request.Caption,
                IsPublic = request.IsPublic,
                CreatedAt = DateTime.UtcNow
            };

            await _shareRepository.CreateAsync(share);

            // Update share count in Post table
            post.ShareCount = await _shareRepository.CountAsync(s => s.PostId == request.PostId);
            await _postRepository.UpdateAsync(post);

            // Get user info for response
            var user = await _userRepository.GetByIdAsync(request.UserId);
            
            var response = share.Adapt<ShareResponse>();
            response.User = user.Adapt<UserResponse>();

            return response;
        }

        public async Task<bool> UnsharePostAsync(int userId, int postId)
        {
            var share = await _shareRepository.GetFirstOrDefaultAsync(
                s => s.UserId == userId && s.PostId == postId);

            if (share == null)
            {
                return false;
            }

            await _shareRepository.DeleteAsync(share);

            // Update share count in Post table
            var post = await _postRepository.GetByIdAsync(postId);
            if (post != null)
            {
                post.ShareCount = await _shareRepository.CountAsync(s => s.PostId == postId);
                await _postRepository.UpdateAsync(post);
            }

            return true;
        }

        public async Task<List<ShareResponse>> GetSharesByPostAsync(int postId)
        {
            var shares = await _shareRepository.GetAllAsync(s => s.PostId == postId);
            var responses = new List<ShareResponse>();

            foreach (var share in shares)
            {
                var user = await _userRepository.GetByIdAsync(share.UserId);
                var response = share.Adapt<ShareResponse>();
                response.User = user.Adapt<UserResponse>();
                responses.Add(response);
            }

            return responses.OrderByDescending(s => s.CreatedAt).ToList();
        }

        public async Task<List<ShareResponse>> GetSharesByUserAsync(int userId)
        {
            var shares = await _shareRepository.GetAllAsync(s => s.UserId == userId);
            var responses = new List<ShareResponse>();

            foreach (var share in shares)
            {
                var user = await _userRepository.GetByIdAsync(share.UserId);
                var response = share.Adapt<ShareResponse>();
                response.User = user.Adapt<UserResponse>();
                responses.Add(response);
            }

            return responses.OrderByDescending(s => s.CreatedAt).ToList();
        }

        public async Task<bool> HasUserSharedPostAsync(int userId, int postId)
        {
            var share = await _shareRepository.GetFirstOrDefaultAsync(
                s => s.UserId == userId && s.PostId == postId);
            return share != null;
        }

        public async Task<int> GetShareCountAsync(int postId)
        {
            return await _shareRepository.CountAsync(s => s.PostId == postId);
        }
    }
}
