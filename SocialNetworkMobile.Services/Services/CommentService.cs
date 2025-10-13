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
        private readonly GenericRepository<Like> _likeRepository;

        public CommentService(
            GenericRepository<Comment> commentRepository,
            GenericRepository<User> userRepository,
            GenericRepository<Post> postRepository,
            GenericRepository<Like> likeRepository)
        {
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _postRepository = postRepository;
            _likeRepository = likeRepository;
        }

        public async Task<CommentResponse> CreateCommentAsync(CreateCommentRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new ArgumentException("User not found");

            var post = await _postRepository.GetByIdAsync(request.PostId);
            if (post == null)
                throw new ArgumentException("Post not found");

            var comment = new Comment
            {
                PostId = request.PostId,
                UserId = request.UserId,
                Content = request.Content,
                ParentCommentId = request.ParentCommentId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _commentRepository.CreateAsync(comment);
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
