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
        public int? GroupId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public UserResponse User { get; set; } = null!;
        public GroupResponse? Group { get; set; }
        public List<TagResponse> Tags { get; set; } = new List<TagResponse>();
        public List<LikeResponse> Likes { get; set; } = new List<LikeResponse>();
        public List<CommentResponse> Comments { get; set; } = new List<CommentResponse>();
        public List<ShareResponse> Shares { get; set; } = new List<ShareResponse>();
        public bool IsLiked { get; set; }
        public bool IsShared { get; set; }
    }
}
