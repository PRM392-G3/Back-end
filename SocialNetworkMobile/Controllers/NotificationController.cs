using Microsoft.AspNetCore.Mvc;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;
using SocialNetworkMobile.Services.Object.Responses;
using Microsoft.AspNetCore.Authorization;

namespace SocialNetworkMobile.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost]
        public async Task<ActionResult<NotificationResponse>> CreateNotification([FromBody] CreateNotificationRequest request)
        {
            try
            {
                var notification = await _notificationService.CreateNotificationAsync(request);
                return CreatedAtAction(nameof(GetNotificationsByUser), new { userId = notification.UserId }, notification);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<NotificationResponse>>> GetNotificationsByUser(int userId)
        {
            try
            {
                var notifications = await _notificationService.GetNotificationsByUserIdAsync(userId);
                return Ok(notifications);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("user/{userId}/unread")]
        public async Task<ActionResult<List<NotificationResponse>>> GetUnreadNotifications(int userId)
        {
            try
            {
                var notifications = await _notificationService.GetUnreadNotificationsAsync(userId);
                return Ok(notifications);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("user/{userId}/unread-count")]
        public async Task<ActionResult> GetUnreadCount(int userId)
        {
            try
            {
                var count = await _notificationService.GetUnreadCountAsync(userId);
                return Ok(new { count });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{notificationId}/mark-read")]
        public async Task<ActionResult> MarkAsRead(int notificationId)
        {
            try
            {
                var result = await _notificationService.MarkAsReadAsync(notificationId);
                if (result)
                    return Ok(new { message = "Notification marked as read" });
                return BadRequest("Unable to mark notification as read");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("user/{userId}/mark-all-read")]
        public async Task<ActionResult> MarkAllAsRead(int userId)
        {
            try
            {
                var result = await _notificationService.MarkAllAsReadAsync(userId);
                if (result)
                    return Ok(new { message = "All notifications marked as read" });
                return BadRequest("Unable to mark all notifications as read");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{notificationId}")]
        public async Task<ActionResult> DeleteNotification(int notificationId)
        {
            try
            {
                var result = await _notificationService.DeleteNotificationAsync(notificationId);
                if (result)
                    return NoContent();
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
