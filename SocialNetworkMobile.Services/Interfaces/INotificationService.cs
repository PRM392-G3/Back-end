using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;

namespace SocialNetworkMobile.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationResponse> CreateNotificationAsync(CreateNotificationRequest request);
        Task<List<NotificationResponse>> GetNotificationsByUserIdAsync(int userId);
        Task<List<NotificationResponse>> GetUnreadNotificationsAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task<bool> DeleteNotificationAsync(int notificationId);
        Task<int> GetUnreadCountAsync(int userId);
    }
}
