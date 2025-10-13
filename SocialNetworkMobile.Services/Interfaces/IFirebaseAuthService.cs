using SocialNetworkMobile.Repository.Models;

namespace SocialNetworkMobile.Services.Interfaces
{
    public interface IFirebaseAuthService
    {
        bool IsUserInRole(string authHeader, string role);
        bool IsUserInPlan(string token, string plan);
        string GenerateJwtToken(User user);
        Task<string> GoogleLoginAsync(string idToken);
    }
}
