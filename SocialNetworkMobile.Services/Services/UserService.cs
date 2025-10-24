using SocialNetworkMobile.Repository.Basic;
using SocialNetworkMobile.Repository.Models;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;
using Mapster;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace SocialNetworkMobile.Services.Services
{
    public class UserService : IUserService
    {
        private readonly GenericRepository<User> _userRepository;
        private readonly GenericRepository<Follow> _followRepository;

        public UserService(GenericRepository<User> userRepository, GenericRepository<Follow> followRepository)
        {
            _userRepository = userRepository;
            _followRepository = followRepository;
        }

        public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
        {
            var existingUser = await _userRepository.GetFirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
                throw new ArgumentException("User with this email already exists");

            var user = new User
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                Bio = request.Bio,
                DateOfBirth = request.DateOfBirth,
                Location = request.Location,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(user);
            return user.Adapt<UserResponse>();
        }

        public async Task<UserResponse> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new ArgumentException("User not found");

            var response = user.Adapt<UserResponse>();
            
            // Get follower and following counts
            var followers = await _followRepository.GetAllAsync(f => f.FollowingId == id);
            var following = await _followRepository.GetAllAsync(f => f.FollowerId == id);
            
            response.FollowersCount = followers.Count;
            response.FollowingCount = following.Count;
            
            return response;
        }

        public async Task<UserResponse> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                throw new ArgumentException("User not found");

            return user.Adapt<UserResponse>();
        }

        public async Task<List<UserResponse>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Adapt<List<UserResponse>>();
        }

        public async Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new ArgumentException("User not found");

            if (!string.IsNullOrEmpty(request.FullName))
                user.FullName = request.FullName;
            if (!string.IsNullOrEmpty(request.Bio))
                user.Bio = request.Bio;
            if (!string.IsNullOrEmpty(request.AvatarUrl))
                user.AvatarUrl = request.AvatarUrl;
            if (!string.IsNullOrEmpty(request.CoverImageUrl))
                user.CoverImageUrl = request.CoverImageUrl;
            if (!string.IsNullOrEmpty(request.PhoneNumber))
                user.PhoneNumber = request.PhoneNumber;
            if (request.DateOfBirth.HasValue)
                user.DateOfBirth = request.DateOfBirth;
            if (!string.IsNullOrEmpty(request.Location))
                user.Location = request.Location;

            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return user.Adapt<UserResponse>();
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return false;

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<UserResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new ArgumentException("Invalid email or password");

            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return user.Adapt<UserResponse>();
        }

        public async Task<UserResponse> GoogleLoginAsync(GoogleLoginRequest request)
        {
            throw new NotImplementedException("Google login implementation needed");
        }

        public async Task<bool> FollowUserAsync(int followerId, int followingId)
        {
            if (followerId == followingId)
                return false;

            var existingFollow = await _followRepository.GetFirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
            if (existingFollow != null)
                return false;

            var follow = new Follow
            {
                FollowerId = followerId,
                FollowingId = followingId,
                CreatedAt = DateTime.UtcNow
            };

            await _followRepository.CreateAsync(follow);
            return true;
        }

        public async Task<bool> UnfollowUserAsync(int followerId, int followingId)
        {
            var follow = await _followRepository.GetFirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
            if (follow == null)
                return false;

            await _followRepository.DeleteAsync(follow);
            return true;
        }

        public async Task<List<UserResponse>> GetFollowersAsync(int userId)
        {
            var followers = await _followRepository.GetAllAsync(f => f.FollowingId == userId);
            var followerIds = followers.Select(f => f.FollowerId).ToList();
            
            var users = await _userRepository.GetAllAsync(u => followerIds.Contains(u.Id));
            var responses = new List<UserResponse>();

            foreach (var user in users)
            {
                var response = user.Adapt<UserResponse>();
                var userFollowers = await _followRepository.GetAllAsync(f => f.FollowingId == user.Id);
                var userFollowing = await _followRepository.GetAllAsync(f => f.FollowerId == user.Id);
                response.FollowersCount = userFollowers.Count;
                response.FollowingCount = userFollowing.Count;
                response.IsFollowing = false; // Followers are not following the current user
                responses.Add(response);
            }

            return responses;
        }

        public async Task<List<UserResponse>> GetFollowingAsync(int userId)
        {
            var following = await _followRepository.GetAllAsync(f => f.FollowerId == userId);
            var followingIds = following.Select(f => f.FollowingId).ToList();
            
            var users = await _userRepository.GetAllAsync(u => followingIds.Contains(u.Id));
            var responses = new List<UserResponse>();

            foreach (var user in users)
            {
                var response = user.Adapt<UserResponse>();
                var userFollowers = await _followRepository.GetAllAsync(f => f.FollowingId == user.Id);
                var userFollowing = await _followRepository.GetAllAsync(f => f.FollowerId == user.Id);
                response.FollowersCount = userFollowers.Count;
                response.FollowingCount = userFollowing.Count;
                response.IsFollowing = true; // These are people the current user is following
                responses.Add(response);
            }

            return responses;
        }

        public async Task<List<UserResponse>> GetUserByNameAsync(string name)
        {
            // Fuzzy search - tìm users có tên chứa từ khóa tìm kiếm
            var users = await _userRepository.GetAllAsync(u => 
                u.FullName.ToLower().Contains(name.ToLower()) && u.IsActive);

            var responses = new List<UserResponse>();
            foreach (var user in users)
            {
                var response = user.Adapt<UserResponse>();
                var userFollowers = await _followRepository.GetAllAsync(f => f.FollowingId == user.Id);
                var userFollowing = await _followRepository.GetAllAsync(f => f.FollowerId == user.Id);
                response.FollowersCount = userFollowers.Count;
                response.FollowingCount = userFollowing.Count;
                response.IsFollowing = false; // Default for search results
                responses.Add(response);
            }

            return responses;
        }


        public async Task<bool> IsFollowingAsync(int followerId, int followingId)
        {
            var follow = await _followRepository.GetFirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
            return follow != null;
        }

        public async Task<List<UserResponse>> GetFollowersWithStatusAsync(int userId, int currentUserId)
        {
            var followers = await _followRepository.GetAllAsync(f => f.FollowingId == userId);
            var followerIds = followers.Select(f => f.FollowerId).ToList();
            
            var users = await _userRepository.GetAllAsync(u => followerIds.Contains(u.Id));
            var responses = new List<UserResponse>();

            foreach (var user in users)
            {
                var response = user.Adapt<UserResponse>();
                var userFollowers = await _followRepository.GetAllAsync(f => f.FollowingId == user.Id);
                var userFollowing = await _followRepository.GetAllAsync(f => f.FollowerId == user.Id);
                response.FollowersCount = userFollowers.Count;
                response.FollowingCount = userFollowing.Count;
                
                // Check if current user is following this user
                var isFollowing = await _followRepository.GetFirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == user.Id);
                response.IsFollowing = isFollowing != null;
                
                responses.Add(response);
            }

            return responses;
        }

        public async Task<List<UserResponse>> GetFollowingWithStatusAsync(int userId, int currentUserId)
        {
            var following = await _followRepository.GetAllAsync(f => f.FollowerId == userId);
            var followingIds = following.Select(f => f.FollowingId).ToList();
            
            var users = await _userRepository.GetAllAsync(u => followingIds.Contains(u.Id));
            var responses = new List<UserResponse>();

            foreach (var user in users)
            {
                var response = user.Adapt<UserResponse>();
                var userFollowers = await _followRepository.GetAllAsync(f => f.FollowingId == user.Id);
                var userFollowing = await _followRepository.GetAllAsync(f => f.FollowerId == user.Id);
                response.FollowersCount = userFollowers.Count;
                response.FollowingCount = userFollowing.Count;
                
                // Check if current user is following this user
                var isFollowing = await _followRepository.GetFirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == user.Id);
                response.IsFollowing = isFollowing != null;
                
                responses.Add(response);
            }

            return responses;
        }
    }
}
