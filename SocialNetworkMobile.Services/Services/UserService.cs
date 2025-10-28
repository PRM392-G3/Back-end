using SocialNetworkMobile.Repository.Basic;
using SocialNetworkMobile.Repository.Models;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;
using Mapster;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore; 
using System.Linq; 
using System.Linq.Expressions; 

namespace SocialNetworkMobile.Services.Services
{
    public class UserService : IUserService
    {
        private readonly GenericRepository<User> _userRepository;
        private readonly GenericRepository<Follow> _followRepository;
        private readonly GenericRepository<Post> _postRepository;
        private readonly INotificationService _notificationService;

        public UserService(
           GenericRepository<User> userRepository,
            GenericRepository<Follow> followRepository,
           GenericRepository<Post> postRepository,
            INotificationService notificationService)
        {
            _userRepository = userRepository;
            _followRepository = followRepository;
            _postRepository = postRepository;
            _notificationService = notificationService;
        }

        // --- Basic User Operations (CRUD, Auth) ---

        public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
        {
            // Check for existing email
            bool userExists = await _userRepository.CountAsync(u => u.Email == request.Email) > 0;
            if (userExists)
            {
                throw new ArgumentException("User with this email already exists");
            }

            var user = new User
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                Bio = request.Bio,
                DateOfBirth = request.DateOfBirth,
                Location = request.Location,
                IsActive = true, // Default to active
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(user);

            return user.Adapt<UserResponse>();
        }

        public async Task<UserResponse> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(u => u.Email == email && u.IsActive);
            if (user == null)
            {
                throw new ArgumentException("User not found or inactive");
            }
            return user.Adapt<UserResponse>();
        }

        public async Task<UserResponse> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || !user.IsActive)
            {
                throw new ArgumentException("User not found or inactive");
            }
            var response = user.Adapt<UserResponse>();
            return response;
        }


        public async Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || !user.IsActive)
            {
                throw new ArgumentException("User not found or inactive");
            }

            // Update fields only if provided in the request
            if (!string.IsNullOrEmpty(request.FullName)) user.FullName = request.FullName;
            if (request.Bio != null) user.Bio = request.Bio; 
            if (request.AvatarUrl != null) user.AvatarUrl = request.AvatarUrl;
            if (request.CoverImageUrl != null) user.CoverImageUrl = request.CoverImageUrl;
            if (!string.IsNullOrEmpty(request.PhoneNumber)) user.PhoneNumber = request.PhoneNumber;
            if (request.DateOfBirth.HasValue) user.DateOfBirth = request.DateOfBirth.Value.ToUniversalTime(); 
            if (request.Location != null) user.Location = request.Location;

            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return user.Adapt<UserResponse>();
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false; 
            }
            if (!user.IsActive)
            {
                return true;
            }

            user.IsActive = false; 
            user.Email = $"deleted_{DateTime.UtcNow.Ticks}_{user.Email}"; 
            user.PhoneNumber = null; 
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<UserResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new ArgumentException("Invalid email or password");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Return the full profile after successful login
            var profile = await GetUserProfileAsync(user.Id, user.Id);

            return profile.Adapt<UserResponse>();
        }

        public async Task<bool> UpdateFcmTokenAsync(int userId, string fcmToken)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                return false; // User not found or inactive
            }
            // Only update if the token is different to avoid unnecessary writes
            if (user.FcmToken != fcmToken)
            {
                user.FcmToken = fcmToken;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
            }
            return true;
        }

        // --- Follow Operations ---

        public async Task<bool> FollowUserAsync(int followerId, int followingId)
        {
            if (followerId == followingId)
            {
                throw new ArgumentException("Cannot follow yourself");
            }
            // Check if already following
            bool alreadyFollowing = await IsFollowingAsync(followerId, followingId);
            if (alreadyFollowing)
            {
                return true; 
            }
            // Check if target user exists and is active
            bool targetUserExists = await _userRepository.CountAsync(u => u.Id == followingId && u.IsActive) > 0;
            if (!targetUserExists)
            {
                throw new ArgumentException("User to follow not found or inactive");
            }
            var followerUser = await _userRepository.GetByIdAsync(followerId);
            var followingUser = await _userRepository.GetByIdAsync(followingId);
            await _followRepository.CreateAsync(new Follow
            {
                FollowerId = followerId,
                FollowingId = followingId,
                CreatedAt = DateTime.UtcNow
            });
            try
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
                {
                    UserId = followingId,         
                    FromUserId = followerId,     
                    Type = "FOLLOW",
                    Title = "Bạn có người theo dõi mới",
                    Message = $"{followerUser.FullName} đã bắt đầu theo dõi bạn."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserService] Error creating FOLLOW notification: {ex.Message}");
            }
            return true;
        }

        public async Task<bool> UnfollowUserAsync(int followerId, int followingId)
        {
            var follow = await _followRepository.GetFirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
            if (follow == null)
            {
                return false; // Not following, nothing to unfollow
            }
            await _followRepository.DeleteAsync(follow);
            return true;
        }

        public async Task<bool> IsFollowingAsync(int followerId, int followingId)
        {
            return await _followRepository.CountAsync(f => f.FollowerId == followerId && f.FollowingId == followingId) > 0;
        }

        // --- Optimized List Operations (Using IQueryable & Projection) ---

        /// <summary>
        /// Gets a paginated list of active users with basic info.
        /// </summary>
        public async Task<List<UserResponse>> GetAllUsersAsync(int page = 1, int limit = 20)
        {
            var users = await _userRepository.GetQueryable()
                .Where(u => u.IsActive)
                .OrderBy(u => u.FullName) 
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    AvatarUrl = u.AvatarUrl,
                    Bio = u.Bio
                })
                .ToListAsync();
            return users;
        }

        /// <summary>
        /// Searches active users by name (paginated) and includes follow counts.
        /// </summary>
        public async Task<List<UserResponse>> GetUserByNameAsync(string name, int page = 1, int limit = 20)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new List<UserResponse>(); 
            }
            var query = name.Trim().ToLower(); 


            var users = await _userRepository.GetQueryable()
                .Where(u => u.FullName.ToLower().Contains(query) && u.IsActive)
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    AvatarUrl = u.AvatarUrl,
                    Bio = u.Bio,
                    FollowersCount = _followRepository.GetQueryable().Count(f => f.FollowingId == u.Id),
                    FollowingCount = _followRepository.GetQueryable().Count(f => f.FollowerId == u.Id),
                    IsFollowing = false
                })
                .ToListAsync();
            return users;
        }

        /// <summary>
        /// Gets user profile including follow/post counts and follow status relative to current user.
        /// Optimized using CountAsync.
        /// </summary>
        public async Task<UserProfileResponse> GetUserProfileAsync(int userId, int? currentUserId = null)
        {
            // Use GetByIdAsync which should handle the existence check
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                throw new ArgumentException("User not found or inactive");
            }

            var profile = user.Adapt<UserProfileResponse>();

            var followersCountTask = _followRepository.CountAsync(f => f.FollowingId == userId);
            var followingCountTask = _followRepository.CountAsync(f => f.FollowerId == userId);
            var postsCountTask = _postRepository.CountAsync(p => p.UserId == userId && !p.IsDeleted); 
            await Task.WhenAll(followersCountTask, followingCountTask, postsCountTask);

            profile.FollowersCount = followersCountTask.Result;
            profile.FollowingCount = followingCountTask.Result;
            profile.PostsCount = postsCountTask.Result;

            if (currentUserId.HasValue && currentUserId.Value != userId)
            {
                profile.IsFollowing = await IsFollowingAsync(currentUserId.Value, userId);
            }
            else
            {
                profile.IsFollowing = false;
            }

            return profile;
        }

        // --- Optimized Follower/Following Lists ---

        /// <summary>
        /// Gets paginated list of followers with counts.
        /// </summary>
        public async Task<List<UserResponse>> GetFollowersAsync(int userId, int page = 1, int limit = 20)
        {
            var followers = await _followRepository.GetQueryable()
                .Where(f => f.FollowingId == userId && f.Follower.IsActive) 
                .Select(f => f.Follower)
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(u => new UserResponse // Project to DTO
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    AvatarUrl = u.AvatarUrl,
                    Bio = u.Bio,
                    FollowersCount = _followRepository.GetQueryable().Count(flw => flw.FollowingId == u.Id),
                    FollowingCount = _followRepository.GetQueryable().Count(flw => flw.FollowerId == u.Id),
                    IsFollowing = false
                })
                .ToListAsync();
            return followers;
        }

        /// <summary>
        /// Gets paginated list of users being followed (following) with counts.
        /// </summary>
        public async Task<List<UserResponse>> GetFollowingAsync(int userId, int page = 1, int limit = 20)
        {
            var following = await _followRepository.GetQueryable()
                .Select(f => f.Following) 
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(u => new UserResponse // Project to DTO
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    AvatarUrl = u.AvatarUrl,
                    Bio = u.Bio,
                    FollowersCount = _followRepository.GetQueryable().Count(flw => flw.FollowingId == u.Id),
                    FollowingCount = _followRepository.GetQueryable().Count(flw => flw.FollowerId == u.Id),
                    IsFollowing = true 
                })
                .ToListAsync();
            return following;
        }

        /// <summary>
        /// Gets paginated followers list with counts and accurate IsFollowing status relative to current user.
        /// </summary>
        public async Task<List<UserResponse>> GetFollowersWithStatusAsync(int userId, int currentUserId, int page = 1, int limit = 20)
        {
            var followers = await _followRepository.GetQueryable()
                .Where(f => f.FollowingId == userId && f.Follower.IsActive)
                .Select(f => f.Follower)
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    AvatarUrl = u.AvatarUrl,
                    Bio = u.Bio,
                    FollowersCount = _followRepository.GetQueryable().Count(flw => flw.FollowingId == u.Id),
                    FollowingCount = _followRepository.GetQueryable().Count(flw => flw.FollowerId == u.Id),
                    IsFollowing = _followRepository.GetQueryable().Any(flw => flw.FollowerId == currentUserId && flw.FollowingId == u.Id)
                })
                .ToListAsync();
            return followers;
        }

        /// <summary>
        /// Gets paginated following list with counts and accurate IsFollowing status relative to current user.
        /// </summary>
        public async Task<List<UserResponse>> GetFollowingWithStatusAsync(int userId, int currentUserId, int page = 1, int limit = 20)
        {
     
            var following = await _followRepository.GetQueryable()
                .Where(f => f.FollowerId == userId && f.Following.IsActive)
                .Select(f => f.Following)
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    AvatarUrl = u.AvatarUrl,
                    Bio = u.Bio,
                    FollowersCount = _followRepository.GetQueryable().Count(flw => flw.FollowingId == u.Id),
                    FollowingCount = _followRepository.GetQueryable().Count(flw => flw.FollowerId == u.Id),
                    IsFollowing = _followRepository.GetQueryable().Any(flw => flw.FollowerId == currentUserId && flw.FollowingId == u.Id)
                })
                .ToListAsync();
            return following;
        }

    }
}