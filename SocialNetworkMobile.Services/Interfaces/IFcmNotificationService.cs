namespace SocialNetworkMobile.Services.Interfaces
{
    public interface IFcmNotificationService
    {
        Task<bool> SendNotificationAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null);
    }
}

