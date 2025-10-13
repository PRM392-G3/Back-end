namespace SocialNetworkMobile.Services.Object.Responses
{
    public class CommentResponse
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int? ParentCommentId { get; set; }
        public int LikeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public UserResponse User { get; set; } = null!;
        public List<CommentResponse> Replies { get; set; } = new List<CommentResponse>();
        public bool IsLiked { get; set; }
    }
}
