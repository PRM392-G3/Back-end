using SocialNetworkMobile.Repository.Basic;
using SocialNetworkMobile.Repository.Models;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;
using Mapster;

namespace SocialNetworkMobile.Services.Services
{
    public class ReelService : IReelService
    {
        private readonly GenericRepository<Reel> _reelRepository;
        private readonly GenericRepository<ReelMusic> _musicRepository;
        private readonly GenericRepository<User> _userRepository;
        private readonly GenericRepository<Like> _likeRepository;
        private readonly GenericRepository<Comment> _commentRepository;

        public ReelService(
            GenericRepository<Reel> reelRepository,
            GenericRepository<ReelMusic> musicRepository,
            GenericRepository<User> userRepository,
            GenericRepository<Like> likeRepository,
            GenericRepository<Comment> commentRepository)
        {
            _reelRepository = reelRepository;
            _musicRepository = musicRepository;
            _userRepository = userRepository;
            _likeRepository = likeRepository;
            _commentRepository = commentRepository;
        }

        public async Task<ReelResponse> CreateReelAsync(CreateReelRequest request)
        {
            // Verify user exists
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new ArgumentException("User not found");

            // Verify music exists if provided
            if (request.MusicId.HasValue)
            {
                var music = await _musicRepository.GetByIdAsync(request.MusicId.Value);
                if (music == null)
                    throw new ArgumentException("Music not found");
            }

            var reel = new Reel
            {
                UserId = request.UserId,
                VideoUrl = request.VideoUrl,
                Caption = request.Caption,
                MusicId = request.MusicId,
                IsPublic = request.IsPublic,
                CreatedAt = DateTime.UtcNow
            };

            await _reelRepository.CreateAsync(reel);
            return await GetReelByIdAsync(reel.Id);
        }

        public async Task<ReelResponse> GetReelByIdAsync(int id)
        {
            var reel = await _reelRepository.GetByIdAsync(id);
            if (reel == null)
                throw new ArgumentException("Reel not found");

            // Manual mapping to ensure null safety
            var response = new ReelResponse
            {
                Id = reel.Id,
                UserId = reel.UserId,
                VideoUrl = reel.VideoUrl ?? string.Empty,
                VideoFileName = reel.VideoFileName,
                Caption = reel.Caption,
                MusicId = reel.MusicId,
                MusicUrl = reel.MusicUrl,
                MusicFileName = reel.MusicFileName,
                MusicTitle = reel.MusicTitle,
                MusicArtist = reel.MusicArtist,
                MusicDuration = reel.MusicDuration,
                Duration = reel.Duration,
                LikeCount = reel.LikeCount,
                CommentCount = reel.CommentCount,
                ShareCount = reel.ShareCount,
                ViewCount = reel.ViewCount,
                IsPublic = reel.IsPublic,
                IsDeleted = reel.IsDeleted,
                CreatedAt = reel.CreatedAt,
                UpdatedAt = reel.UpdatedAt
            };
            
            // Get user info
            var user = await _userRepository.GetByIdAsync(reel.UserId);
            if (user != null)
            {
                response.User = new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    AvatarUrl = user.AvatarUrl,
                    CoverImageUrl = user.CoverImageUrl,
                    PhoneNumber = user.PhoneNumber,
                    Bio = user.Bio,
                    DateOfBirth = user.DateOfBirth,
                    Location = user.Location,
                    IsActive = user.IsActive,
                    EmailVerifiedAt = user.EmailVerifiedAt,
                    LastLoginAt = user.LastLoginAt,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    FollowersCount = 0,
                    FollowingCount = 0,
                    PostsCount = 0,
                    IsFollowing = false
                };
            }
            
            // Get music if present
            if (reel.MusicId.HasValue)
            {
                var music = await _musicRepository.GetByIdAsync(reel.MusicId.Value);
                if (music != null)
                    response.Music = music.Adapt<ReelMusicResponse>();
            }

            return response;
        }

        public async Task<List<ReelResponse>> GetAllReelsAsync(int? currentUserId = null)
        {
            try
            {
                var reels = await _reelRepository.GetAllAsync();
                
                // Filter reels based on privacy settings
                var filteredReels = reels.Where(r => r.IsDeleted == false && (
                    r.IsPublic == true || // Show all public reels
                    (r.IsPublic == false && currentUserId.HasValue && r.UserId == currentUserId.Value) // Show private reels only for their owner
                )).OrderByDescending(r => r.CreatedAt).ToList();
                
                // Get all likes for current user if authenticated
                var userLikes = new HashSet<int>();
                if (currentUserId.HasValue)
                {
                    var allLikes = await _likeRepository.GetAllAsync();
                    userLikes = new HashSet<int>(allLikes
                        .Where(l => l.UserId == currentUserId.Value && l.ReelId.HasValue)
                        .Select(l => l.ReelId!.Value));
                }
                
                var responses = new List<ReelResponse>();
                foreach (var reel in filteredReels)
                {
                    // Manual mapping to ensure null safety
                    var response = new ReelResponse
                    {
                        Id = reel.Id,
                        UserId = reel.UserId,
                        VideoUrl = reel.VideoUrl ?? string.Empty,
                        VideoFileName = reel.VideoFileName,
                        Caption = reel.Caption,
                        MusicId = reel.MusicId,
                        MusicUrl = reel.MusicUrl,
                        MusicFileName = reel.MusicFileName,
                        MusicTitle = reel.MusicTitle,
                        MusicArtist = reel.MusicArtist,
                        MusicDuration = reel.MusicDuration,
                        Duration = reel.Duration,
                        LikeCount = reel.LikeCount,
                        CommentCount = reel.CommentCount,
                        ShareCount = reel.ShareCount,
                        ViewCount = reel.ViewCount,
                        IsPublic = reel.IsPublic,
                        IsDeleted = reel.IsDeleted,
                        CreatedAt = reel.CreatedAt,
                        UpdatedAt = reel.UpdatedAt,
                        IsLiked = userLikes.Contains(reel.Id) // Check if current user liked this reel
                    };
                    
                    // Get user info
                    var user = await _userRepository.GetByIdAsync(reel.UserId);
                    if (user != null)
                    {
                        response.User = new UserResponse
                        {
                            Id = user.Id,
                            Email = user.Email ?? string.Empty,
                            FullName = user.FullName,
                            AvatarUrl = user.AvatarUrl,
                            CoverImageUrl = user.CoverImageUrl,
                            PhoneNumber = user.PhoneNumber,
                            Bio = user.Bio,
                            DateOfBirth = user.DateOfBirth,
                            Location = user.Location,
                            IsActive = user.IsActive,
                            EmailVerifiedAt = user.EmailVerifiedAt,
                            LastLoginAt = user.LastLoginAt,
                            CreatedAt = user.CreatedAt,
                            UpdatedAt = user.UpdatedAt,
                            FollowersCount = 0,
                            FollowingCount = 0,
                            PostsCount = 0,
                            IsFollowing = false
                        };
                    }
                    
                    // Get music if present
                    if (reel.MusicId.HasValue)
                    {
                        var music = await _musicRepository.GetByIdAsync(reel.MusicId.Value);
                        if (music != null)
                            response.Music = music.Adapt<ReelMusicResponse>();
                    }
                    
                    responses.Add(response);
                }
                
                return responses;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error in GetAllReelsAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<List<ReelResponse>> GetReelsByUserIdAsync(int userId)
        {
            var allReels = await _reelRepository.GetAllAsync();
            var reels = allReels.Where(r => r.UserId == userId && r.IsPublic && !r.IsDeleted).OrderByDescending(r => r.CreatedAt).ToList();
            
            var responses = new List<ReelResponse>();
            foreach (var reel in reels)
            {
                // Manual mapping to ensure null safety
                var response = new ReelResponse
                {
                    Id = reel.Id,
                    UserId = reel.UserId,
                    VideoUrl = reel.VideoUrl ?? string.Empty,
                    VideoFileName = reel.VideoFileName,
                    Caption = reel.Caption,
                    MusicId = reel.MusicId,
                    MusicUrl = reel.MusicUrl,
                    MusicFileName = reel.MusicFileName,
                    MusicTitle = reel.MusicTitle,
                    MusicArtist = reel.MusicArtist,
                    MusicDuration = reel.MusicDuration,
                    Duration = reel.Duration,
                    LikeCount = reel.LikeCount,
                    CommentCount = reel.CommentCount,
                    ShareCount = reel.ShareCount,
                    ViewCount = reel.ViewCount,
                    IsPublic = reel.IsPublic,
                    IsDeleted = reel.IsDeleted,
                    CreatedAt = reel.CreatedAt,
                    UpdatedAt = reel.UpdatedAt
                };
                
                // Get user info
                var user = await _userRepository.GetByIdAsync(reel.UserId);
                if (user != null)
                {
                    response.User = new UserResponse
                    {
                        Id = user.Id,
                        Email = user.Email ?? string.Empty,
                        FullName = user.FullName,
                        AvatarUrl = user.AvatarUrl,
                        CoverImageUrl = user.CoverImageUrl,
                        PhoneNumber = user.PhoneNumber,
                        Bio = user.Bio,
                        DateOfBirth = user.DateOfBirth,
                        Location = user.Location,
                        IsActive = user.IsActive,
                        EmailVerifiedAt = user.EmailVerifiedAt,
                        LastLoginAt = user.LastLoginAt,
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt,
                        FollowersCount = 0,
                        FollowingCount = 0,
                        PostsCount = 0,
                        IsFollowing = false
                    };
                }
                
                // Get music if present
                if (reel.MusicId.HasValue)
                {
                    var music = await _musicRepository.GetByIdAsync(reel.MusicId.Value);
                    if (music != null)
                        response.Music = music.Adapt<ReelMusicResponse>();
                }
                
                responses.Add(response);
            }
            
            return responses;
        }

        public async Task<ReelResponse> UpdateReelAsync(int id, int userId, UpdateReelRequest request)
        {
            var reel = await _reelRepository.GetByIdAsync(id);
            if (reel == null)
                throw new ArgumentException("Reel not found");

            // Check if user owns the reel
            if (reel.UserId != userId)
                throw new UnauthorizedAccessException("You don't have permission to update this reel");

            // Update only provided fields
            if (request.Caption != null)
                reel.Caption = request.Caption;
            
            if (request.IsPublic.HasValue)
                reel.IsPublic = request.IsPublic.Value;
            
            if (request.VideoUrl != null)
                reel.VideoUrl = request.VideoUrl;
            
            if (request.VideoFileName != null)
                reel.VideoFileName = request.VideoFileName;
            
            if (request.MusicId.HasValue)
                reel.MusicId = request.MusicId;
            
            if (request.MusicUrl != null)
                reel.MusicUrl = request.MusicUrl;
            
            if (request.MusicFileName != null)
                reel.MusicFileName = request.MusicFileName;
            
            if (request.MusicTitle != null)
                reel.MusicTitle = request.MusicTitle;
            
            if (request.MusicArtist != null)
                reel.MusicArtist = request.MusicArtist;
            
            if (request.MusicDuration.HasValue)
                reel.MusicDuration = request.MusicDuration.Value;
            
            if (request.Duration.HasValue)
                reel.Duration = request.Duration.Value;

            reel.UpdatedAt = DateTime.UtcNow;
            await _reelRepository.UpdateAsync(reel);
            
            return await GetReelByIdAsync(reel.Id);
        }

        public async Task<bool> DeleteReelAsync(int id, int userId)
        {
            var reel = await _reelRepository.GetByIdAsync(id);
            if (reel == null)
                return false;

            // Check if user owns the reel
            if (reel.UserId != userId)
                throw new UnauthorizedAccessException("You don't have permission to delete this reel");

            // Delete all comments related to this reel first
            var comments = await _commentRepository.GetAllAsync();
            var reelComments = comments.Where(c => c.ReelId == id).ToList();
            
            foreach (var comment in reelComments)
            {
                await _commentRepository.DeleteAsync(comment);
            }

            // Delete all likes related to this reel
            var likes = await _likeRepository.GetAllAsync();
            var reelLikes = likes.Where(l => l.ReelId == id).ToList();
            
            foreach (var like in reelLikes)
            {
                await _likeRepository.DeleteAsync(like);
            }

            // Now delete the reel
            await _reelRepository.DeleteAsync(reel);
            return true;
        }

        // Music management methods
        public async Task<ReelMusicResponse> CreateMusicAsync(CreateReelMusicRequest request)
        {
            var music = new ReelMusic
            {
                Title = request.Title,
                Artist = request.Artist,
                MusicUrl = request.MusicUrl,
                Duration = request.Duration,
                CoverImageUrl = request.CoverImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _musicRepository.CreateAsync(music);
            return music.Adapt<ReelMusicResponse>();
        }

        public async Task<List<ReelMusicResponse>> GetAllMusicAsync()
        {
            var musics = await _musicRepository.GetAllAsync();
            return musics.Adapt<List<ReelMusicResponse>>();
        }

        public async Task<ReelMusicResponse> GetMusicByIdAsync(int id)
        {
            var music = await _musicRepository.GetByIdAsync(id);
            if (music == null)
                throw new ArgumentException("Music not found");

            return music.Adapt<ReelMusicResponse>();
        }

        public async Task<bool> LikeReelAsync(int userId, int reelId)
        {
            // Check if user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found");

            // Check if reel exists
            var reel = await _reelRepository.GetByIdAsync(reelId);
            if (reel == null)
                throw new ArgumentException("Reel not found");

            // Check if user already liked this reel
            var existingLikes = await _likeRepository.GetAllAsync();
            var existingLike = existingLikes.FirstOrDefault(l => l.UserId == userId && l.ReelId == reelId);
            
            if (existingLike != null)
                return true; // Already liked

            // Create new like
            var like = new Like
            {
                UserId = userId,
                ReelId = reelId,
                LikeType = "LIKE",
                CreatedAt = DateTime.UtcNow
            };

            await _likeRepository.CreateAsync(like);

            // Update reel like count
            reel.LikeCount++;
            reel.UpdatedAt = DateTime.UtcNow;
            await _reelRepository.UpdateAsync(reel);

            return true;
        }

        public async Task<bool> UnlikeReelAsync(int userId, int reelId)
        {
            // Get existing like
            var existingLikes = await _likeRepository.GetAllAsync();
            var existingLike = existingLikes.FirstOrDefault(l => l.UserId == userId && l.ReelId == reelId);
            
            if (existingLike == null)
                return true; // Not liked, nothing to do

            // Delete the like
            await _likeRepository.DeleteAsync(existingLike);

            // Update reel like count
            var reel = await _reelRepository.GetByIdAsync(reelId);
            if (reel != null)
            {
                reel.LikeCount = Math.Max(0, reel.LikeCount - 1);
                reel.UpdatedAt = DateTime.UtcNow;
                await _reelRepository.UpdateAsync(reel);
            }

            return true;
        }

        public async Task<bool> UpdateCommentCountAsync(int reelId)
        {
            var reel = await _reelRepository.GetByIdAsync(reelId);
            if (reel == null)
                return false;

            // Count comments for this reel
            var comments = await _commentRepository.GetAllAsync();
            var commentCount = comments.Count(c => c.ReelId == reelId && !c.IsDeleted);

            reel.CommentCount = commentCount;
            reel.UpdatedAt = DateTime.UtcNow;
            await _reelRepository.UpdateAsync(reel);

            return true;
        }
    }
}
