namespace SocialNetworkMobile.Services.Object.Responses
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserResponse User { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}
