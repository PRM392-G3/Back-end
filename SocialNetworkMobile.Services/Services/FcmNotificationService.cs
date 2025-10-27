using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using SocialNetworkMobile.Services.Interfaces;

namespace SocialNetworkMobile.Services.Services
{
    public class FcmNotificationService : IFcmNotificationService
    {
        public async Task<bool> SendNotificationAsync(string fcmToken, string title, string body, Dictionary<string, string>? data = null)
        {
            try
            {
                // Check if FCM token is valid
                if (string.IsNullOrWhiteSpace(fcmToken))
                {
                    Console.WriteLine($"[FcmNotificationService] FCM token is null or empty");
                    return false;
                }

                // Check if Firebase is initialized
                if (FirebaseMessaging.DefaultInstance == null)
                {
                    Console.WriteLine($"[FcmNotificationService] Firebase is not initialized");
                    return false;
                }

                Console.WriteLine($"[FcmNotificationService] Sending notification to token: {fcmToken.Substring(0, Math.Min(20, fcmToken.Length))}...");
                Console.WriteLine($"[FcmNotificationService] Title: {title}");
                Console.WriteLine($"[FcmNotificationService] Body: {body}");

                var message = new Message
                {
                    Token = fcmToken,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data?.ToDictionary(d => d.Key, d => d.Value) ?? new Dictionary<string, string>()
                };

                var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                Console.WriteLine($"[FcmNotificationService] ✅ Successfully sent notification. Message ID: {response}");
                return true;
            }
            catch (FirebaseMessagingException ex)
            {
                Console.WriteLine($"[FcmNotificationService] ❌ Firebase error: {ex.Message}");
                Console.WriteLine($"[FcmNotificationService] Error code: {ex.ErrorCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FcmNotificationService] ❌ Error sending notification: {ex.Message}");
                Console.WriteLine($"[FcmNotificationService] Stack trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}

