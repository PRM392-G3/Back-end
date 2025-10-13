using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;

namespace SocialNetworkMobile.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request);
        Task<bool> ValidateTokenAsync(string token);
        Task<UserResponse> GetUserFromTokenAsync(string token);
        Task<string> GenerateTokenAsync(int userId);
        Task<bool> LogoutAsync(string token);
    }
}
