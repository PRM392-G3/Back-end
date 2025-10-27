using SocialNetworkMobile.Repository.Basic;
using SocialNetworkMobile.Repository.Models;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;
using Mapster;

namespace SocialNetworkMobile.Services.Services
{
    public class CommentService : ICommentService
    {
        private readonly GenericRepository<Comment> _commentRepository;
        private readonly GenericRepository<User> _userRepository;
        private readonly GenericRepository<Post> _postRepository;
        private readonly GenericRepository<Reel> _reelRepository;
        private readonly GenericRepository<Like> _likeRepository;
        private readonly INotificationService _notificationService;

        public CommentService(
            GenericRepository<Comment> commentRepository,
            GenericRepository<User> userRepository,
            GenericRepository<Post> postRepository,
            GenericRepository<Reel> reelRepository,
            GenericRepository<Like> likeRepository,
            INotificationService notificationService)
        {
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _postRepository = postRepository;
            _reelRepository = reelRepository;
            _likeRepository = likeRepository;
            _notificationService = notificationService;
        }

        public async Task<CommentResponse> CreateCommentAsync(CreateCommentRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new ArgumentException("User not found");

            // Validate that either PostId or ReelId is provided, but not both
            if (request.PostId.HasValue && request.ReelId.HasValue)
                throw new ArgumentException("Comment cannot be associated with both a post and a reel");
            
            if (!request.PostId.HasValue && !request.ReelId.HasValue)
                throw new ArgumentException("Comment must be associated with either a post or a reel");

            // Validate post exists if PostId is provided
            if (request.PostId.HasValue)
            {
                var post = await _postRepository.GetByIdAsync(request.PostId.Value);
                if (post == null)
                    throw new ArgumentException("Post not found");
            }

            // Validate reel exists if ReelId is provided
            if (request.ReelId.HasValue)
            {
                var reel = await _reelRepository.GetByIdAsync(request.ReelId.Value);
                if (reel == null)
                    throw new ArgumentException("Reel not found");
            }

            var comment = new Comment
            {
                PostId = request.PostId,
                ReelId = request.ReelId,
                UserId = request.UserId,
                Content = request.Content,
                ParentCommentId = request.ParentCommentId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _commentRepository.CreateAsync(comment);
            
            // Update comment count for the associated post or reel
            if (comment.PostId.HasValue)
            {
                // ✅ Tạo notification cho post owner (và tự động gửi FCM)
                var post = await _postRepository.GetByIdAsync(comment.PostId.Value);
                if (post != null && post.UserId != request.UserId) // Không notify chính mình
                {
                    try
                    {
                        var commenter = await _userRepository.GetByIdAsync(request.UserId);
                        Console.WriteLine($"[CommentService] Creating notification for comment on post {comment.PostId.Value}");
                        await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
                        {
                            UserId = post.UserId, // Post owner
                            FromUserId = request.UserId, // Commenter
                            Type = "COMMENT",
                            Title = "Bình luận mới",
                            Message = $"{commenter?.FullName} đã bình luận: {request.Content.Substring(0, Math.Min(50, request.Content.Length))}...",
                            PostId = comment.PostId.Value,
                            CommentId = comment.Id
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[CommentService] Error creating notification: {ex.Message}");
                        // Don't throw - notification failure shouldn't break comment creation
                    }
                }
            }
            else if (comment.ReelId.HasValue)
            {
                // Update reel comment count
                var reel = await _reelRepository.GetByIdAsync(comment.ReelId.Value);
                if (reel != null)
                {
                    reel.CommentCount++;
                    reel.UpdatedAt = DateTime.UtcNow;
                    await _reelRepository.UpdateAsync(reel);
                }
            }
            
            return await GetCommentByIdAsync(comment.Id);
        }

        public async Task<CommentResponse> GetCommentByIdAsync(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
                throw new ArgumentException("Comment not found");

            var response = comment.Adapt<CommentResponse>();
            
            // Get user info
            var user = await _userRepository.GetByIdAsync(comment.UserId);
            response.User = user.Adapt<UserResponse>();

            // Get replies if any
            var replies = await _commentRepository.GetAllAsync(c => c.ParentCommentId == id && c.IsDeleted == false);
            var replyResponses = new List<CommentResponse>();

            foreach (var reply in replies)
            {
                var replyResponse = reply.Adapt<CommentResponse>();
                var replyUser = await _userRepository.GetByIdAsync(reply.UserId);
                replyResponse.User = replyUser.Adapt<UserResponse>();
                replyResponses.Add(replyResponse);
            }

            response.Replies = replyResponses;

            return response;
        }

        public async Task<List<CommentResponse>> GetCommentsByPostIdAsync(int postId)
        {
            var comments = await _commentRepository.GetAllAsync(c => c.PostId == postId && c.ParentCommentId == null && c.IsDeleted == false);
            var responses = new List<CommentResponse>();

            foreach (var comment in comments)
            {
                var response = comment.Adapt<CommentResponse>();
                
                // Get user info
                var user = await _userRepository.GetByIdAsync(comment.UserId);
                response.User = user.Adapt<UserResponse>();

                // Get replies
                var replies = await _commentRepository.GetAllAsync(c => c.ParentCommentId == comment.Id && c.IsDeleted == false);
                var replyResponses = new List<CommentResponse>();

                foreach (var reply in replies)
                {
                    var replyResponse = reply.Adapt<CommentResponse>();
                    var replyUser = await _userRepository.GetByIdAsync(reply.UserId);
                    replyResponse.User = replyUser.Adapt<UserResponse>();
                    replyResponses.Add(replyResponse);
                }

                response.Replies = replyResponses;
                responses.Add(response);
            }

            return responses.OrderByDescending(c => c.CreatedAt).ToList();
        }

        public async Task<List<CommentResponse>> GetCommentsByReelIdAsync(int reelId)
        {
            var comments = await _commentRepository.GetAllAsync(c => c.ReelId == reelId && c.ParentCommentId == null && c.IsDeleted == false);
            var responses = new List<CommentResponse>();

            foreach (var comment in comments)
            {
                var response = comment.Adapt<CommentResponse>();
                
                // Get user info
                var user = await _userRepository.GetByIdAsync(comment.UserId);
                response.User = user.Adapt<UserResponse>();

                // Get replies
                var replies = await _commentRepository.GetAllAsync(c => c.ParentCommentId == comment.Id && c.IsDeleted == false);
                var replyResponses = new List<CommentResponse>();

                foreach (var reply in replies)
                {
                    var replyResponse = reply.Adapt<CommentResponse>();
                    var replyUser = await _userRepository.GetByIdAsync(reply.UserId);
                    replyResponse.User = replyUser.Adapt<UserResponse>();
                    replyResponses.Add(replyResponse);
                }

                response.Replies = replyResponses;
                responses.Add(response);
            }

            return responses.OrderByDescending(c => c.CreatedAt).ToList();
        }

        public async Task<List<CommentResponse>> GetRepliesByCommentIdAsync(int commentId)
        {
            var replies = await _commentRepository.GetAllAsync(c => c.ParentCommentId == commentId && c.IsDeleted == false);
            var responses = new List<CommentResponse>();

            foreach (var reply in replies)
            {
                var response = reply.Adapt<CommentResponse>();
                
                // Get user info
                var user = await _userRepository.GetByIdAsync(reply.UserId);
                response.User = user.Adapt<UserResponse>();

                responses.Add(response);
            }

            return responses.OrderByDescending(c => c.CreatedAt).ToList();
        }

        public async Task<CommentResponse> UpdateCommentAsync(int id, UpdateCommentRequest request)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
                throw new ArgumentException("Comment not found");

            comment.Content = request.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            await _commentRepository.UpdateAsync(comment);
            return await GetCommentByIdAsync(id);
        }

        public async Task<bool> DeleteCommentAsync(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
                return false;

            comment.IsDeleted = true;
            comment.UpdatedAt = DateTime.UtcNow;
            await _commentRepository.UpdateAsync(comment);
            return true;
        }

        public async Task<bool> LikeCommentAsync(int userId, int commentId)
        {
            var existingLike = await _likeRepository.GetFirstOrDefaultAsync(l => l.UserId == userId && l.CommentId == commentId);
            if (existingLike != null)
                return false;

            var like = new Like
            {
                UserId = userId,
                CommentId = commentId,
                LikeType = "LIKE",
                CreatedAt = DateTime.UtcNow
            };

            await _likeRepository.CreateAsync(like);
            return true;
        }

        public async Task<bool> UnlikeCommentAsync(int userId, int commentId)
        {
            var like = await _likeRepository.GetFirstOrDefaultAsync(l => l.UserId == userId && l.CommentId == commentId);
            if (like == null)
                return false;

            await _likeRepository.DeleteAsync(like);
            return true;
        }
    }
}
