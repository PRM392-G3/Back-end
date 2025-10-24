using SocialNetworkMobile.Repository.Basic;
using SocialNetworkMobile.Repository.Models;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace SocialNetworkMobile.Services.Services
{
    public class PostService : IPostService
    {
        private readonly GenericRepository<Post> _postRepository;
        private readonly GenericRepository<User> _userRepository;
        private readonly GenericRepository<Like> _likeRepository;
        private readonly GenericRepository<Comment> _commentRepository;
        private readonly GenericRepository<Tag> _tagRepository;
        private readonly GenericRepository<PostTag> _postTagRepository;
        private readonly GenericRepository<Share> _shareRepository;

        public PostService(
            GenericRepository<Post> postRepository,
            GenericRepository<User> userRepository,
            GenericRepository<Like> likeRepository,
            GenericRepository<Comment> commentRepository,
            GenericRepository<Tag> tagRepository,
            GenericRepository<PostTag> postTagRepository,
            GenericRepository<Share> shareRepository)
        {
            _postRepository = postRepository;
            _userRepository = userRepository;
            _likeRepository = likeRepository;
            _commentRepository = commentRepository;
            _tagRepository = tagRepository;
            _postTagRepository = postTagRepository;
            _shareRepository = shareRepository;
        }

        public async Task<PostResponse> CreatePostAsync(CreatePostRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new ArgumentException("User not found");

            var post = new Post
            {
                UserId = request.UserId,
                Content = request.Content,
                ImageUrl = request.ImageUrl,
                VideoUrl = request.VideoUrl,
                IsPublic = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _postRepository.CreateAsync(post);

            // Handle tags
            if (request.Tags != null && request.Tags.Any())
            {
                foreach (var tagName in request.Tags)
                {
                    var tag = await _tagRepository.GetFirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower());
                    if (tag == null)
                    {
                        tag = new Tag
                        {
                            Name = tagName,
                            UsageCount = 1,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _tagRepository.CreateAsync(tag);
                    }

                    var postTag = new PostTag
                    {
                        PostId = post.Id,
                        TagId = tag.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _postTagRepository.CreateAsync(postTag);
                }
            }

            return await GetPostByIdAsync(post.Id);
        }

        public async Task<PostResponse> GetPostByIdAsync(int id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
                throw new ArgumentException("Post not found");

            var response = post.Adapt<PostResponse>();
            
            // Get user info
            var user = await _userRepository.GetByIdAsync(post.UserId);
            response.User = user.Adapt<UserResponse>();

            // Get tags
            var postTags = await _postTagRepository.GetAllAsync(pt => pt.PostId == id);
            var tagIds = postTags.Select(pt => pt.TagId).ToList();
            var tags = await _tagRepository.GetAllAsync(t => tagIds.Contains(t.Id));
            response.Tags = tags.Adapt<List<TagResponse>>();

            // Get likes with user info
            var likes = await _likeRepository.GetAllAsync(l => l.PostId == id);
            var likeResponses = new List<LikeResponse>();
            foreach (var like in likes)
            {
                var likeUser = await _userRepository.GetByIdAsync(like.UserId);
                likeResponses.Add(new LikeResponse
                {
                    Id = like.Id,
                    PostId = like.PostId ?? 0,
                    UserId = like.UserId,
                    LikeType = like.LikeType,
                    CreatedAt = like.CreatedAt,
                    User = likeUser.Adapt<UserResponse>()
                });
            }
            response.Likes = likeResponses;
            
            // Get comments with user info
            var comments = await _commentRepository.GetAllAsync(c => c.PostId == id);
            var commentResponses = new List<CommentResponse>();
            foreach (var comment in comments)
            {
                var commentUser = await _userRepository.GetByIdAsync(comment.UserId);
                commentResponses.Add(new CommentResponse
                {
                    Id = comment.Id,
                    PostId = comment.PostId,
                    UserId = comment.UserId,
                    Content = comment.Content,
                    ParentCommentId = comment.ParentCommentId ?? 0,
                    LikeCount = comment.LikeCount,
                    CreatedAt = comment.CreatedAt,
                    UpdatedAt = comment.UpdatedAt,
                    User = commentUser.Adapt<UserResponse>(),
                    Replies = new List<CommentResponse>(),
                    IsLiked = false
                });
            }
            response.Comments = commentResponses;

            // Get shares with user info
            var shares = await _shareRepository.GetAllAsync(s => s.PostId == id);
            var shareResponses = new List<ShareResponse>();
            foreach (var share in shares)
            {
                var shareUser = await _userRepository.GetByIdAsync(share.UserId);
                shareResponses.Add(new ShareResponse
                {
                    Id = share.Id,
                    UserId = share.UserId,
                    PostId = share.PostId,
                    Caption = share.Caption,
                    IsPublic = share.IsPublic,
                    CreatedAt = share.CreatedAt,
                    User = shareUser.Adapt<UserResponse>()
                });
            }
            response.Shares = shareResponses;

            return response;
        }

        public async Task<List<PostResponse>> GetAllPostsAsync()
        {
            var posts = await _postRepository.GetAllAsync(p => p.IsDeleted == false && p.IsPublic == true);
            var responses = new List<PostResponse>();

            foreach (var post in posts)
            {
                var response = post.Adapt<PostResponse>();
                
                // Get user info
                var user = await _userRepository.GetByIdAsync(post.UserId);
                response.User = user.Adapt<UserResponse>();

                // Get tags
                var postTags = await _postTagRepository.GetAllAsync(pt => pt.PostId == post.Id);
                var tagIds = postTags.Select(pt => pt.TagId).ToList();
                var tags = await _tagRepository.GetAllAsync(t => tagIds.Contains(t.Id));
                response.Tags = tags.Adapt<List<TagResponse>>();

                // Get likes with user info
                var likes = await _likeRepository.GetAllAsync(l => l.PostId == post.Id);
                var likeResponses = new List<LikeResponse>();
                foreach (var like in likes)
                {
                    var likeUser = await _userRepository.GetByIdAsync(like.UserId);
                    likeResponses.Add(new LikeResponse
                    {
                        Id = like.Id,
                        PostId = like.PostId ?? 0,
                        UserId = like.UserId,
                        LikeType = like.LikeType,
                        CreatedAt = like.CreatedAt,
                        User = likeUser.Adapt<UserResponse>()
                    });
                }
                response.Likes = likeResponses;
                response.LikeCount = likeResponses.Count;
                response.IsLiked = false; // Default for GetAllPosts - no current user context
                
                // Get comments with user info
                var comments = await _commentRepository.GetAllAsync(c => c.PostId == post.Id);
                var commentResponses = new List<CommentResponse>();
                foreach (var comment in comments)
                {
                    var commentUser = await _userRepository.GetByIdAsync(comment.UserId);
                    commentResponses.Add(new CommentResponse
                    {
                        Id = comment.Id,
                        PostId = comment.PostId,
                        UserId = comment.UserId,
                        Content = comment.Content,
                        ParentCommentId = comment.ParentCommentId ?? 0,
                        LikeCount = comment.LikeCount,
                        CreatedAt = comment.CreatedAt,
                        UpdatedAt = comment.UpdatedAt,
                        User = commentUser.Adapt<UserResponse>(),
                        Replies = new List<CommentResponse>(),
                        IsLiked = false
                    });
                }
                response.Comments = commentResponses;

                // Get shares with user info
                var shares = await _shareRepository.GetAllAsync(s => s.PostId == post.Id);
                var shareResponses = new List<ShareResponse>();
                foreach (var share in shares)
                {
                    var shareUser = await _userRepository.GetByIdAsync(share.UserId);
                    shareResponses.Add(new ShareResponse
                    {
                        Id = share.Id,
                        UserId = share.UserId,
                        PostId = share.PostId,
                        Caption = share.Caption,
                        IsPublic = share.IsPublic,
                        CreatedAt = share.CreatedAt,
                        User = shareUser.Adapt<UserResponse>()
                    });
                }
                response.Shares = shareResponses;

                responses.Add(response);
            }

            return responses.OrderByDescending(p => p.CreatedAt).ToList();
        }

        public async Task<List<PostResponse>> GetPostsByUserIdAsync(int userId)
        {
            var posts = await _postRepository.GetAllAsync(p => p.UserId == userId && p.IsDeleted == false);
            var responses = new List<PostResponse>();

            foreach (var post in posts)
            {
                var response = post.Adapt<PostResponse>();
                
                // Get user info
                var user = await _userRepository.GetByIdAsync(post.UserId);
                response.User = user.Adapt<UserResponse>();

                // Get tags
                var postTags = await _postTagRepository.GetAllAsync(pt => pt.PostId == post.Id);
                var tagIds = postTags.Select(pt => pt.TagId).ToList();
                var tags = await _tagRepository.GetAllAsync(t => tagIds.Contains(t.Id));
                response.Tags = tags.Adapt<List<TagResponse>>();

                // Get shares with user info
                var shares = await _shareRepository.GetAllAsync(s => s.PostId == post.Id);
                var shareResponses = new List<ShareResponse>();
                foreach (var share in shares)
                {
                    var shareUser = await _userRepository.GetByIdAsync(share.UserId);
                    shareResponses.Add(new ShareResponse
                    {
                        Id = share.Id,
                        UserId = share.UserId,
                        PostId = share.PostId,
                        Caption = share.Caption,
                        IsPublic = share.IsPublic,
                        CreatedAt = share.CreatedAt,
                        User = shareUser.Adapt<UserResponse>()
                    });
                }
                response.Shares = shareResponses;

                responses.Add(response);
            }

            return responses.OrderByDescending(p => p.CreatedAt).ToList();
        }

        public async Task<List<PostResponse>> GetPostsByUserIdWithLikesAsync(int userId, int currentUserId)
        {
            var posts = await _postRepository.GetAllAsync(p => p.UserId == userId && p.IsDeleted == false);
            var responses = new List<PostResponse>();

            foreach (var post in posts)
            {
                var response = post.Adapt<PostResponse>();
                
                // Get user info
                var user = await _userRepository.GetByIdAsync(post.UserId);
                response.User = user.Adapt<UserResponse>();

                // Get tags
                var postTags = await _postTagRepository.GetAllAsync(pt => pt.PostId == post.Id);
                var tagIds = postTags.Select(pt => pt.TagId).ToList();
                var tags = await _tagRepository.GetAllAsync(t => tagIds.Contains(t.Id));
                response.Tags = tags.Adapt<List<TagResponse>>();

                // Get likes with user info
                var likes = await _likeRepository.GetAllAsync(l => l.PostId == post.Id);
                var likeResponses = new List<LikeResponse>();
                foreach (var like in likes)
                {
                    var likeUser = await _userRepository.GetByIdAsync(like.UserId);
                    likeResponses.Add(new LikeResponse
                    {
                        Id = like.Id,
                        PostId = like.PostId ?? 0,
                        UserId = like.UserId,
                        LikeType = like.LikeType,
                        CreatedAt = like.CreatedAt,
                        User = likeUser.Adapt<UserResponse>()
                    });
                }
                response.Likes = likeResponses;
                response.LikeCount = likeResponses.Count;
                
                // Check if current user has liked this post
                response.IsLiked = await _likeRepository.GetAllAsync(l => l.PostId == post.Id && l.UserId == currentUserId).ContinueWith(t => t.Result.Any());
                
                // Get comments with user info
                var comments = await _commentRepository.GetAllAsync(c => c.PostId == post.Id);
                var commentResponses = new List<CommentResponse>();
                foreach (var comment in comments)
                {
                    var commentUser = await _userRepository.GetByIdAsync(comment.UserId);
                    commentResponses.Add(new CommentResponse
                    {
                        Id = comment.Id,
                        PostId = comment.PostId,
                        UserId = comment.UserId,
                        Content = comment.Content,
                        ParentCommentId = comment.ParentCommentId ?? 0,
                        CreatedAt = comment.CreatedAt,
                        User = commentUser.Adapt<UserResponse>()
                    });
                }
                response.Comments = commentResponses;
                response.CommentCount = commentResponses.Count;

                // Get shares with user info
                var shares = await _shareRepository.GetAllAsync(s => s.PostId == post.Id);
                var shareResponses = new List<ShareResponse>();
                foreach (var share in shares)
                {
                    var shareUser = await _userRepository.GetByIdAsync(share.UserId);
                    shareResponses.Add(new ShareResponse
                    {
                        Id = share.Id,
                        PostId = share.PostId,
                        UserId = share.UserId,
                        Caption = share.Caption,
                        IsPublic = share.IsPublic,
                        CreatedAt = share.CreatedAt,
                        User = shareUser.Adapt<UserResponse>()
                    });
                }
                response.Shares = shareResponses;
                response.ShareCount = shareResponses.Count;
                response.IsShared = await _shareRepository.GetAllAsync(s => s.PostId == post.Id && s.UserId == currentUserId).ContinueWith(t => t.Result.Any());

                responses.Add(response);
            }

            return responses.OrderByDescending(p => p.CreatedAt).ToList();
        }

        public async Task<List<PostResponse>> GetSharedPostsByUserIdAsync(int userId)
        {
            // Get all shares by user
            var shares = await _shareRepository.GetAllAsync(s => s.UserId == userId);
            var postIds = shares.Select(s => s.PostId).ToList();
            
            if (!postIds.Any())
                return new List<PostResponse>();

            // Get posts that were shared
            var posts = await _postRepository.GetAllAsync(p => postIds.Contains(p.Id) && p.IsDeleted == false);
            var responses = new List<PostResponse>();

            foreach (var post in posts)
            {
                var response = post.Adapt<PostResponse>();
                
                // Get user info
                var user = await _userRepository.GetByIdAsync(post.UserId);
                response.User = user.Adapt<UserResponse>();

                // Get tags
                var postTags = await _postTagRepository.GetAllAsync(pt => pt.PostId == post.Id);
                var tagIds = postTags.Select(pt => pt.TagId).ToList();
                var tags = await _tagRepository.GetAllAsync(t => tagIds.Contains(t.Id));
                response.Tags = tags.Adapt<List<TagResponse>>();

                // Get shares with user info
                var postShares = await _shareRepository.GetAllAsync(s => s.PostId == post.Id);
                var shareResponses = new List<ShareResponse>();
                foreach (var share in postShares)
                {
                    var shareUser = await _userRepository.GetByIdAsync(share.UserId);
                    shareResponses.Add(new ShareResponse
                    {
                        Id = share.Id,
                        UserId = share.UserId,
                        PostId = share.PostId,
                        Caption = share.Caption,
                        IsPublic = share.IsPublic,
                        CreatedAt = share.CreatedAt,
                        User = shareUser.Adapt<UserResponse>()
                    });
                }
                response.Shares = shareResponses;

                responses.Add(response);
            }

            return responses.OrderByDescending(p => p.CreatedAt).ToList();
        }

        public async Task<List<PostResponse>> GetSharedPostsByUserIdWithLikesAsync(int userId, int currentUserId)
        {
            // Get all shares by user
            var shares = await _shareRepository.GetAllAsync(s => s.UserId == userId);
            var postIds = shares.Select(s => s.PostId).ToList();
            
            if (!postIds.Any())
                return new List<PostResponse>();

            // Get posts that were shared
            var posts = await _postRepository.GetAllAsync(p => postIds.Contains(p.Id) && p.IsDeleted == false);
            var responses = new List<PostResponse>();

            foreach (var post in posts)
            {
                var response = post.Adapt<PostResponse>();
                
                // Get user info
                var user = await _userRepository.GetByIdAsync(post.UserId);
                response.User = user.Adapt<UserResponse>();

                // Get tags
                var postTags = await _postTagRepository.GetAllAsync(pt => pt.PostId == post.Id);
                var tagIds = postTags.Select(pt => pt.TagId).ToList();
                var tags = await _tagRepository.GetAllAsync(t => tagIds.Contains(t.Id));
                response.Tags = tags.Adapt<List<TagResponse>>();

                // Get likes with user info
                var likes = await _likeRepository.GetAllAsync(l => l.PostId == post.Id);
                var likeResponses = new List<LikeResponse>();
                foreach (var like in likes)
                {
                    var likeUser = await _userRepository.GetByIdAsync(like.UserId);
                    likeResponses.Add(new LikeResponse
                    {
                        Id = like.Id,
                        PostId = like.PostId ?? 0,
                        UserId = like.UserId,
                        LikeType = like.LikeType,
                        CreatedAt = like.CreatedAt,
                        User = likeUser.Adapt<UserResponse>()
                    });
                }
                response.Likes = likeResponses;
                response.LikeCount = likeResponses.Count;
                
                // Check if current user has liked this post
                response.IsLiked = await _likeRepository.GetAllAsync(l => l.PostId == post.Id && l.UserId == currentUserId).ContinueWith(t => t.Result.Any());
                
                // Get comments with user info
                var comments = await _commentRepository.GetAllAsync(c => c.PostId == post.Id);
                var commentResponses = new List<CommentResponse>();
                foreach (var comment in comments)
                {
                    var commentUser = await _userRepository.GetByIdAsync(comment.UserId);
                    commentResponses.Add(new CommentResponse
                    {
                        Id = comment.Id,
                        PostId = comment.PostId,
                        UserId = comment.UserId,
                        Content = comment.Content,
                        ParentCommentId = comment.ParentCommentId ?? 0,
                        CreatedAt = comment.CreatedAt,
                        User = commentUser.Adapt<UserResponse>()
                    });
                }
                response.Comments = commentResponses;
                response.CommentCount = commentResponses.Count;

                // Get shares with user info
                var postShares = await _shareRepository.GetAllAsync(s => s.PostId == post.Id);
                var shareResponses = new List<ShareResponse>();
                foreach (var share in postShares)
                {
                    var shareUser = await _userRepository.GetByIdAsync(share.UserId);
                    shareResponses.Add(new ShareResponse
                    {
                        Id = share.Id,
                        PostId = share.PostId,
                        UserId = share.UserId,
                        Caption = share.Caption,
                        IsPublic = share.IsPublic,
                        CreatedAt = share.CreatedAt,
                        User = shareUser.Adapt<UserResponse>()
                    });
                }
                response.Shares = shareResponses;
                response.ShareCount = shareResponses.Count;
                response.IsShared = await _shareRepository.GetAllAsync(s => s.PostId == post.Id && s.UserId == currentUserId).ContinueWith(t => t.Result.Any());

                responses.Add(response);
            }

            return responses.OrderByDescending(p => p.CreatedAt).ToList();
        }

        public async Task<List<PostResponse>> GetFeedPostsAsync(int userId, int page = 1, int pageSize = 10)
        {
            // For now, return all public posts. In a real app, you'd filter by followed users
            var posts = await _postRepository.GetAllAsync(p => p.IsDeleted == false && p.IsPublic == true);
            var responses = new List<PostResponse>();

            foreach (var post in posts)
            {
                var response = post.Adapt<PostResponse>();
                
                // Get user info
                var user = await _userRepository.GetByIdAsync(post.UserId);
                response.User = user.Adapt<UserResponse>();

                // Get tags
                var postTags = await _postTagRepository.GetAllAsync(pt => pt.PostId == post.Id);
                var tagIds = postTags.Select(pt => pt.TagId).ToList();
                var tags = await _tagRepository.GetAllAsync(t => tagIds.Contains(t.Id));
                response.Tags = tags.Adapt<List<TagResponse>>();

                // Get shares with user info
                var shares = await _shareRepository.GetAllAsync(s => s.PostId == post.Id);
                var shareResponses = new List<ShareResponse>();
                foreach (var share in shares)
                {
                    var shareUser = await _userRepository.GetByIdAsync(share.UserId);
                    shareResponses.Add(new ShareResponse
                    {
                        Id = share.Id,
                        UserId = share.UserId,
                        PostId = share.PostId,
                        Caption = share.Caption,
                        IsPublic = share.IsPublic,
                        CreatedAt = share.CreatedAt,
                        User = shareUser.Adapt<UserResponse>()
                    });
                }
                response.Shares = shareResponses;

                responses.Add(response);
            }

            return responses
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public async Task<PostResponse> UpdatePostAsync(int id, UpdatePostRequest request)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
                throw new ArgumentException("Post not found");

            if (!string.IsNullOrEmpty(request.Content))
                post.Content = request.Content;
            if (!string.IsNullOrEmpty(request.ImageUrl))
                post.ImageUrl = request.ImageUrl;
            if (!string.IsNullOrEmpty(request.VideoUrl))
                post.VideoUrl = request.VideoUrl;

            post.UpdatedAt = DateTime.UtcNow;
            await _postRepository.UpdateAsync(post);

            // Handle tags update
            if (request.Tags != null)
            {
                // Remove existing tags
                var existingPostTags = await _postTagRepository.GetAllAsync(pt => pt.PostId == id);
                foreach (var postTag in existingPostTags)
                {
                    await _postTagRepository.DeleteAsync(postTag);
                }

                // Add new tags
                foreach (var tagName in request.Tags)
                {
                    var tag = await _tagRepository.GetFirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower());
                    if (tag == null)
                    {
                        tag = new Tag
                        {
                            Name = tagName,
                            UsageCount = 1,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _tagRepository.CreateAsync(tag);
                    }

                    var postTag = new PostTag
                    {
                        PostId = id,
                        TagId = tag.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _postTagRepository.CreateAsync(postTag);
                }
            }

            return await GetPostByIdAsync(id);
        }

        public async Task<bool> DeletePostAsync(int id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
                return false;

            post.IsDeleted = true;
            post.UpdatedAt = DateTime.UtcNow;
            await _postRepository.UpdateAsync(post);
            return true;
        }

        public async Task<bool> LikePostAsync(int userId, int postId)
        {
            var existingLike = await _likeRepository.GetFirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);
            if (existingLike != null)
                return false;

            var like = new Like
            {
                UserId = userId,
                PostId = postId,
                LikeType = "LIKE",
                CreatedAt = DateTime.UtcNow
            };

            await _likeRepository.CreateAsync(like);

            // Update like count in Post table
            var post = await _postRepository.GetByIdAsync(postId);
            if (post != null)
            {
                post.LikeCount = await _likeRepository.CountAsync(l => l.PostId == postId);
                await _postRepository.UpdateAsync(post);
            }

            return true;
        }

        public async Task<bool> UnlikePostAsync(int userId, int postId)
        {
            var like = await _likeRepository.GetFirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);
            if (like == null)
                return false;

            await _likeRepository.DeleteAsync(like);

            // Update like count in Post table
            var post = await _postRepository.GetByIdAsync(postId);
            if (post != null)
            {
                post.LikeCount = await _likeRepository.CountAsync(l => l.PostId == postId);
                await _postRepository.UpdateAsync(post);
            }

            return true;
        }

        public async Task<List<PostResponse>> SearchPostsAsync(string searchTerm)
        {
            var posts = await _postRepository.GetAllAsync(p => 
                p.IsDeleted == false && 
                p.IsPublic == true && 
                (p.Content.Contains(searchTerm) || p.Content.ToLower().Contains(searchTerm.ToLower())));

            var responses = new List<PostResponse>();

            foreach (var post in posts)
            {
                var response = post.Adapt<PostResponse>();
                
                // Get user info
                var user = await _userRepository.GetByIdAsync(post.UserId);
                response.User = user.Adapt<UserResponse>();

                // Get tags
                var postTags = await _postTagRepository.GetAllAsync(pt => pt.PostId == post.Id);
                var tagIds = postTags.Select(pt => pt.TagId).ToList();
                var tags = await _tagRepository.GetAllAsync(t => tagIds.Contains(t.Id));
                response.Tags = tags.Adapt<List<TagResponse>>();

                responses.Add(response);
            }

            return responses.OrderByDescending(p => p.CreatedAt).ToList();
        }

        public async Task<List<PostResponse>> GetPostsByTagAsync(string tagName)
        {
            var tag = await _tagRepository.GetFirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower());
            if (tag == null)
                return new List<PostResponse>();

            var postTags = await _postTagRepository.GetAllAsync(pt => pt.TagId == tag.Id);
            var postIds = postTags.Select(pt => pt.PostId).ToList();
            var posts = await _postRepository.GetAllAsync(p => postIds.Contains(p.Id) && p.IsDeleted == false && p.IsPublic == true);

            var responses = new List<PostResponse>();

            foreach (var post in posts)
            {
                var response = post.Adapt<PostResponse>();
                
                // Get user info
                var user = await _userRepository.GetByIdAsync(post.UserId);
                response.User = user.Adapt<UserResponse>();

                // Get tags
                var postTagsForPost = await _postTagRepository.GetAllAsync(pt => pt.PostId == post.Id);
                var tagIds = postTagsForPost.Select(pt => pt.TagId).ToList();
                var tags = await _tagRepository.GetAllAsync(t => tagIds.Contains(t.Id));
                response.Tags = tags.Adapt<List<TagResponse>>();

                responses.Add(response);
            }

            return responses.OrderByDescending(p => p.CreatedAt).ToList();
        }

        public async Task<List<UserResponse>> GetPostLikesAsync(int postId)
        {
            // Check if post exists
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
                throw new ArgumentException("Post not found");

            // Get all likes for this post
            var likes = await _likeRepository.GetAllAsync(l => l.PostId == postId);
            
            if (!likes.Any())
                return new List<UserResponse>();

            // Get user IDs who liked the post
            var userIds = likes.Select(l => l.UserId).ToList();
            
            // Get users who liked the post
            var users = await _userRepository.GetAllAsync(u => userIds.Contains(u.Id));
            
            // Convert to UserResponse
            var userResponses = users.Select(u => u.Adapt<UserResponse>()).ToList();
            
            return userResponses.OrderByDescending(u => u.CreatedAt).ToList();
        }

        public async Task<List<PostResponse>> GetAllPostsWithLikesAsync(int currentUserId)
        {
            var posts = await _postRepository.GetAllAsync(p => p.IsDeleted == false && p.IsPublic == true);
            var responses = new List<PostResponse>();

            foreach (var post in posts)
            {
                var response = post.Adapt<PostResponse>();
                
                // Get user info
                var user = await _userRepository.GetByIdAsync(post.UserId);
                response.User = user.Adapt<UserResponse>();

                // Get tags
                var postTags = await _postTagRepository.GetAllAsync(pt => pt.PostId == post.Id);
                var tagIds = postTags.Select(pt => pt.TagId).ToList();
                var tags = await _tagRepository.GetAllAsync(t => tagIds.Contains(t.Id));
                response.Tags = tags.Adapt<List<TagResponse>>();

                // Get likes with user info
                var likes = await _likeRepository.GetAllAsync(l => l.PostId == post.Id);
                var likeResponses = new List<LikeResponse>();
                foreach (var like in likes)
                {
                    var likeUser = await _userRepository.GetByIdAsync(like.UserId);
                    likeResponses.Add(new LikeResponse
                    {
                        Id = like.Id,
                        PostId = like.PostId ?? 0,
                        UserId = like.UserId,
                        LikeType = like.LikeType,
                        CreatedAt = like.CreatedAt,
                        User = likeUser.Adapt<UserResponse>()
                    });
                }
                response.Likes = likeResponses;
                response.LikeCount = likeResponses.Count;
                
                // Check if current user has liked this post
                response.IsLiked = await _likeRepository.GetAllAsync(l => l.PostId == post.Id && l.UserId == currentUserId).ContinueWith(t => t.Result.Any());
                
                // Get comments with user info
                var comments = await _commentRepository.GetAllAsync(c => c.PostId == post.Id);
                var commentResponses = new List<CommentResponse>();
                foreach (var comment in comments)
                {
                    var commentUser = await _userRepository.GetByIdAsync(comment.UserId);
                    commentResponses.Add(new CommentResponse
                    {
                        Id = comment.Id,
                        PostId = comment.PostId,
                        UserId = comment.UserId,
                        Content = comment.Content,
                        ParentCommentId = comment.ParentCommentId ?? 0,
                        CreatedAt = comment.CreatedAt,
                        User = commentUser.Adapt<UserResponse>()
                    });
                }
                response.Comments = commentResponses;
                response.CommentCount = commentResponses.Count;

                // Get shares
                var shares = await _shareRepository.GetAllAsync(s => s.PostId == post.Id);
                var shareResponses = new List<ShareResponse>();
                foreach (var share in shares)
                {
                    var shareUser = await _userRepository.GetByIdAsync(share.UserId);
                    shareResponses.Add(new ShareResponse
                    {
                        Id = share.Id,
                        PostId = share.PostId,
                        UserId = share.UserId,
                        Caption = share.Caption,
                        IsPublic = share.IsPublic,
                        CreatedAt = share.CreatedAt,
                        User = shareUser.Adapt<UserResponse>()
                    });
                }
                response.Shares = shareResponses;
                response.ShareCount = shareResponses.Count;
                response.IsShared = await _shareRepository.GetAllAsync(s => s.PostId == post.Id && s.UserId == currentUserId).ContinueWith(t => t.Result.Any());

                responses.Add(response);
            }

            return responses;
        }
    }
}
