namespace SocialNetworkMobile.Services.Object.Responses
{
    /// <summary>
    /// Lightweight DTO cho feed - chỉ chứa thông tin cơ bản để hiển thị list
    /// Comments được lazy load khi user bấm vào
    /// </summary>
    public class PostFeedResponse
    {
        // Basic post info
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        
        // Basic user info (không load full profile)
        public string UserName { get; set; } = string.Empty;
        public string UserAvatar { get; set; } = string.Empty;
        
        // Counts only (không load full list)
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int ShareCount { get; set; }
        
        // Status flags
        public bool IsLiked { get; set; }
        public bool IsShared { get; set; }
        public bool IsPublic { get; set; }
        
        // Tags (only names, not full objects)
        public List<string> TagNames { get; set; } = new List<string>();
    }
}

