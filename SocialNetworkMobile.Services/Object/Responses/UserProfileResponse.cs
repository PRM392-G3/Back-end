namespace SocialNetworkMobile.Services.Object.Responses
{
    /// <summary>
    /// ✅ Lightweight profile DTO - only essential info for fast loading
    /// Used for profile preview/overview - not full details
    /// </summary>
    public class UserProfileResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Bio { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public int PostsCount { get; set; }
        public bool IsFollowing { get; set; }
    }
}

