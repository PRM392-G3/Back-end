using SocialNetworkMobile.Repository.Basic;
using SocialNetworkMobile.Repository.Models;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;
using Mapster;

namespace SocialNetworkMobile.Services.Services
{
    public class NotificationService : INotificationService
    {
        private readonly GenericRepository<Notification> _notificationRepository;
        private readonly GenericRepository<User> _userRepository;

        public NotificationService(
            GenericRepository<Notification> notificationRepository,
            GenericRepository<User> userRepository)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
        }

        public async Task<NotificationResponse> CreateNotificationAsync(CreateNotificationRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new ArgumentException("User not found");

            var fromUser = await _userRepository.GetByIdAsync(request.FromUserId);
            if (fromUser == null)
                throw new ArgumentException("From user not found");

            var notification = new Notification
            {
                UserId = request.UserId,
                FromUserId = request.FromUserId,
                Type = request.Type,
                Title = request.Title,
                Message = request.Message,
                PostId = request.PostId,
                CommentId = request.CommentId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.CreateAsync(notification);
            return await GetNotificationByIdAsync(notification.Id);
        }

        public async Task<List<NotificationResponse>> GetNotificationsByUserIdAsync(int userId)
        {
            var notifications = await _notificationRepository.GetAllAsync(n => n.UserId == userId);
            var responses = new List<NotificationResponse>();

            foreach (var notification in notifications)
            {
                var response = notification.Adapt<NotificationResponse>();
                
                // Get from user info
                var fromUser = await _userRepository.GetByIdAsync(notification.FromUserId);
                response.FromUser = fromUser.Adapt<UserResponse>();

                responses.Add(response);
            }

            return responses.OrderByDescending(n => n.CreatedAt).ToList();
        }

        public async Task<List<NotificationResponse>> GetUnreadNotificationsAsync(int userId)
        {
            var notifications = await _notificationRepository.GetAllAsync(n => n.UserId == userId && n.IsRead == false);
            var responses = new List<NotificationResponse>();

            foreach (var notification in notifications)
            {
                var response = notification.Adapt<NotificationResponse>();
                
                // Get from user info
                var fromUser = await _userRepository.GetByIdAsync(notification.FromUserId);
                response.FromUser = fromUser.Adapt<UserResponse>();

                responses.Add(response);
            }

            return responses.OrderByDescending(n => n.CreatedAt).ToList();
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null)
                return false;

            notification.IsRead = true;
            await _notificationRepository.UpdateAsync(notification);
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            var notifications = await _notificationRepository.GetAllAsync(n => n.UserId == userId && n.IsRead == false);
            
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                await _notificationRepository.UpdateAsync(notification);
            }

            return true;
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null)
                return false;

            await _notificationRepository.DeleteAsync(notification);
            return true;
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            var notifications = await _notificationRepository.GetAllAsync(n => n.UserId == userId && n.IsRead == false);
            return notifications.Count;
        }

        private async Task<NotificationResponse> GetNotificationByIdAsync(int id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
                throw new ArgumentException("Notification not found");

            var response = notification.Adapt<NotificationResponse>();
            
            // Get from user info
            var fromUser = await _userRepository.GetByIdAsync(notification.FromUserId);
            response.FromUser = fromUser.Adapt<UserResponse>();

            return response;
        }
    }
}
