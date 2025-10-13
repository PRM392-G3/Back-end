namespace SocialNetworkMobile.Services.Object.Responses
{
    public class PostResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int ShareCount { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public UserResponse User { get; set; } = null!;
        public List<TagResponse> Tags { get; set; } = new List<TagResponse>();
        public bool IsLiked { get; set; }
    }
}
