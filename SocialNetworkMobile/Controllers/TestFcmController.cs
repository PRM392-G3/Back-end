using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetworkMobile.Services.Interfaces;
using SocialNetworkMobile.Services.Object.Requests;

namespace SocialNetworkMobile.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestFcmController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;
        private readonly IFcmNotificationService _fcmNotificationService;

        public TestFcmController(INotificationService notificationService, IUserService userService, IFcmNotificationService fcmNotificationService)
        {
            _notificationService = notificationService;
            _userService = userService;
            _fcmNotificationService = fcmNotificationService;
        }

        /// <summary>
        /// ✅ TEST: Kiểm tra FcmToken của user
        /// </summary>
        [HttpGet("check-fcm-token/{userId}")]
        public async Task<ActionResult> CheckFcmToken(int userId)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                bool hasToken = !string.IsNullOrEmpty(user.FcmToken);
                
                return Ok(new 
                { 
                    hasFcmToken = hasToken,
                    fcmTokenLength = hasToken ? user.FcmToken?.Length : 0,
                    message = hasToken 
                        ? "User has FCM token ✅" 
                        : "User does NOT have FCM token ❌ - Cannot send push notification!"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// ✅ TEST: Gửi test notification để kiểm tra FCM
        /// </summary>
        [HttpPost("send-test/{userId}/{fromUserId}")]
        // [Authorize]
        public async Task<ActionResult> SendTestNotification(int userId, int fromUserId, [FromBody] string? message = null)
        {
            try
            {
                Console.WriteLine($"[TestFcmController] ========== TEST NOTIFICATION ==========");
                Console.WriteLine($"[TestFcmController] Sending test notification from user {fromUserId} to user {userId}");
                
                // Check if user has FCM token
                var user = await _userService.GetUserByIdAsync(userId);
                if (string.IsNullOrEmpty(user.FcmToken))
                {
                    Console.WriteLine($"[TestFcmController] ❌ User {userId} does NOT have FCM token");
                    return Ok(new 
                    { 
                        success = false,
                        message = "User does not have FCM token. Check server console log for details.",
                        warning = "Push notification will NOT be sent to this user!",
                        action = "Please update FCM token from mobile app"
                    });
                }
                
                Console.WriteLine($"[TestFcmController] ✅ User {userId} has FCM token (length: {user.FcmToken.Length})");
                
                Console.WriteLine($"[TestFcmController] Creating notification in database...");
                await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
                {
                    UserId = userId,
                    FromUserId = fromUserId,
                    Type = "TEST",
                    Title = "Test Notification",
                    Message = message ?? "Đây là thông báo test để kiểm tra FCM"
                });
                
                Console.WriteLine($"[TestFcmController] ========== TEST NOTIFICATION COMPLETE ==========");

                return Ok(new 
                { 
                    success = true,
                    message = "Test notification sent successfully.",
                    fcmTokenLength = user.FcmToken.Length,
                    note = "Check mobile device for push notification. If not received, check server console for FCM logs."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TestFcmController] ❌ Error: {ex.Message}");
                Console.WriteLine($"[TestFcmController] Stack trace: {ex.StackTrace}");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// ✅ TEST: Gửi push notification TRỰC TIẾP qua FCM + tạo notification trong database
        /// </summary>
        [HttpPost("send-push-direct/{userId}/{fromUserId}")]
        public async Task<ActionResult> SendPushDirect(int userId, int fromUserId, string? title = null, string? body = null)
        {
            try
            {
                Console.WriteLine($"[TestFcmController] ========== DIRECT FCM PUSH TEST (with in-app) ==========");
                Console.WriteLine($"[TestFcmController] Sending from user {fromUserId} to user {userId}");
                
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return BadRequest(new { error = "User not found" });
                }

                if (string.IsNullOrEmpty(user.FcmToken))
                {
                    Console.WriteLine($"[TestFcmController] ❌ User {userId} does NOT have FCM token");
                    return Ok(new 
                    { 
                        success = false,
                        message = "User does not have FCM token",
                        action = "Please update FCM token from mobile app"
                    });
                }

                Console.WriteLine($"[TestFcmController] ✅ User {userId} has FCM token (length: {user.FcmToken.Length})");
                
                // Tạo notification trong database (in-app notification)
                Console.WriteLine($"[TestFcmController] Creating in-app notification in database...");
                await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
                {
                    UserId = userId,
                    FromUserId = fromUserId,
                    Type = "TEST_DIRECT",
                    Title = title ?? "Test Push Notification",
                    Message = body ?? "Đây là thông báo test push notification"
                });
                Console.WriteLine($"[TestFcmController] ✅ In-app notification created successfully");
                
                // Gửi FCM push notification
                Console.WriteLine($"[TestFcmController] Sending FCM push notification...");
                var result = await _fcmNotificationService.SendNotificationAsync(
                    user.FcmToken,
                    title ?? "Test Push Notification",
                    body ?? "Đây là thông báo test push notification",
                    new Dictionary<string, string> 
                    { 
                        { "Type", "TEST_DIRECT" },
                        { "UserId", userId.ToString() }
                    }
                );

                if (result)
                {
                    Console.WriteLine($"[TestFcmController] ✅ FCM push notification sent successfully");
                    return Ok(new 
                    { 
                        success = true,
                        message = "Both in-app notification and FCM push sent successfully!",
                        inAppNotification = true,
                        fcmPushNotification = true,
                        fcmTokenLength = user.FcmToken.Length,
                        note = "Check mobile device for both in-app notification and push pop-up."
                    });
                }
                else
                {
                    Console.WriteLine($"[TestFcmController] ⚠️ FCM push notification FAILED but in-app notification created");
                    return Ok(new 
                    { 
                        success = true,
                        message = "In-app notification created, but FCM push notification FAILED.",
                        inAppNotification = true,
                        fcmPushNotification = false,
                        fcmTokenLength = user.FcmToken.Length,
                        note = "Check server console for FCM error details."
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TestFcmController] ❌ Error: {ex.Message}");
                Console.WriteLine($"[TestFcmController] Stack trace: {ex.StackTrace}");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// ✅ TEST: Gửi push notification trực tiếp với custom FCM token
        /// </summary>
        [HttpPost("send-push-custom")]
        public async Task<ActionResult> SendPushCustom([FromBody] dynamic request)
        {
            try
            {
                Console.WriteLine($"[TestFcmController] ========== CUSTOM FCM PUSH TEST ==========");
                
                string? fcmToken = request.fcmToken?.ToString();
                string? title = request.title?.ToString() ?? "Test Push Notification";
                string? body = request.body?.ToString() ?? "Đây là thông báo test push notification";
                
                if (string.IsNullOrEmpty(fcmToken))
                {
                    return BadRequest(new { error = "FCM token is required" });
                }

                Console.WriteLine($"[TestFcmController] FCM token length: {fcmToken.Length}");
                Console.WriteLine($"[TestFcmController] Sending DIRECT FCM push notification...");

                var result = await _fcmNotificationService.SendNotificationAsync(
                    fcmToken,
                    title,
                    body,
                    new Dictionary<string, string> 
                    { 
                        { "Type", "TEST_CUSTOM" }
                    }
                );

                if (result)
                {
                    return Ok(new 
                    { 
                        success = true,
                        message = "FCM push notification sent successfully! Check mobile device for pop-up."
                    });
                }
                else
                {
                    return Ok(new 
                    { 
                        success = false,
                        message = "FCM push notification FAILED. Check server console for details."
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TestFcmController] ❌ Error: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}

