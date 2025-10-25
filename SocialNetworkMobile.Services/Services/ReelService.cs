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

        public ReelService(
            GenericRepository<Reel> reelRepository,
            GenericRepository<ReelMusic> musicRepository,
            GenericRepository<User> userRepository)
        {
            _reelRepository = reelRepository;
            _musicRepository = musicRepository;
            _userRepository = userRepository;
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

        public async Task<List<ReelResponse>> GetAllReelsAsync()
        {
            try
            {
                var reels = await _reelRepository.GetAllAsync();
                var publicReels = reels.Where(r => r.IsPublic && r.IsDeleted == false).OrderByDescending(r => r.CreatedAt).ToList();
                
                var responses = new List<ReelResponse>();
                foreach (var reel in publicReels)
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

        public async Task<bool> DeleteReelAsync(int id, int userId)
        {
            var reel = await _reelRepository.GetByIdAsync(id);
            if (reel == null)
                return false;

            // Check if user owns the reel
            if (reel.UserId != userId)
                throw new UnauthorizedAccessException("You don't have permission to delete this reel");

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
    }
}
