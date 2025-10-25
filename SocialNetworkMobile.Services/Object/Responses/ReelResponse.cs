namespace SocialNetworkMobile.Services.Object.Responses
{
    public class ReelResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string VideoUrl { get; set; } = string.Empty;
        public string? VideoFileName { get; set; }
        public string? Caption { get; set; }
        public int? MusicId { get; set; }
        public string? MusicUrl { get; set; }
        public string? MusicFileName { get; set; }
        public string? MusicTitle { get; set; }
        public string? MusicArtist { get; set; }
        public int MusicDuration { get; set; }
        public int Duration { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int ShareCount { get; set; }
        public int ViewCount { get; set; }
        public bool IsPublic { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public UserResponse? User { get; set; }
        public ReelMusicResponse? Music { get; set; }
    }
}
