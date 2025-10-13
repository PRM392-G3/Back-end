using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Supabase;
using Supabase.Storage;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SocialNetworkMobile.Controllers
{
    [ApiController]
    [Route("api/blob-storage")]
    [Produces("application/json")]
    [Authorize]
    public class BlobStorageController : ControllerBase
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly ILogger<BlobStorageController> _logger;
        private readonly IConfiguration _configuration;

        public BlobStorageController(Supabase.Client supabaseClient, IConfiguration configuration, ILogger<BlobStorageController> logger)
        {
            _supabaseClient = supabaseClient;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Test Supabase connection (No authentication required)
        /// </summary>
        /// <returns>Connection test result</returns>
        [HttpGet("test-connection")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Test Supabase connection", Description = "Tests the connection to Supabase storage")]
        [SwaggerResponse(200, "Connection successful", typeof(object))]
        [SwaggerResponse(500, "Connection failed", typeof(object))]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
               // Check Supabase configuration
               var url = _configuration["Supabase:Url"];
               var anonKey = _configuration["Supabase:AnonKey"];
               var serviceRoleKey = _configuration["Supabase:ServiceRoleKey"];
               
               _logger.LogInformation("Testing Supabase connection with URL: {Url}", url);
               _logger.LogInformation("ServiceRoleKey starts with: {ServiceKeyStart}", serviceRoleKey?.Substring(0, Math.Min(20, serviceRoleKey?.Length ?? 0)));
               
               if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(serviceRoleKey))
               {
                   return BadRequest(new { Message = "Supabase configuration is incomplete. ServiceRoleKey is required." });
               }

                // Initialize Supabase storage
                _logger.LogInformation("Initializing Supabase client for test...");
                await _supabaseClient.InitializeAsync();
                var storage = _supabaseClient.Storage;
                var bucketName = _configuration["Supabase:Storage:BucketName"] ?? "media";

                // Test connection by listing buckets
                _logger.LogInformation("Attempting to list buckets...");
                var buckets = await storage.ListBuckets();
                
                _logger.LogInformation("Successfully retrieved {Count} buckets", buckets?.Count ?? 0);
                
                return Ok(new { 
                    Message = "Supabase connection successful", 
                    BucketCount = buckets?.Count ?? 0,
                    Buckets = buckets?.Select(b => b.Name).ToList() ?? new List<string>(),
                    TargetBucket = bucketName,
                    Url = url
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Supabase connection test failed: {Error}", ex.Message);
                _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                return StatusCode(500, new { Message = $"Connection test failed: {ex.Message}", Details = ex.ToString() });
            }
        }

        /// <summary>
        /// Upload a file (image, video, document) to Supabase storage
        /// </summary>
        /// <param name="file">File to upload</param>
        /// <param name="folder">Optional folder path (default: "uploads")</param>
        /// <returns>Upload result with file URL</returns>
        [HttpPost("media/upload")]
        [SwaggerOperation(Summary = "Upload file", Description = "Uploads a file to Supabase storage")]
        [SwaggerResponse(200, "File uploaded successfully", typeof(FileUploadResponse))]
        [SwaggerResponse(400, "Invalid file or request", typeof(object))]
        [SwaggerResponse(500, "Upload failed", typeof(object))]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string folder = "uploads")
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { Message = "No file provided." });
                }

                // Allow all file types - no restriction
                // File type validation removed as requested

                // Check file size (max 50MB)
                if (file.Length > 50 * 1024 * 1024)
                {
                    return BadRequest(new { Message = "File size cannot exceed 50MB." });
                }

                // Get user ID from JWT token
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "User not authenticated." });
                }

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = $"{folder}/{userId}/{fileName}";

                // Check Supabase configuration
                var url = _configuration["Supabase:Url"];
                var serviceRoleKey = _configuration["Supabase:ServiceRoleKey"];
                
                if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(serviceRoleKey) ||
                    url.Contains("your-project-ref") || serviceRoleKey.Contains("your-supabase-service-role-key"))
                {
                    return BadRequest(new { Message = "Supabase configuration is incomplete. ServiceRoleKey is required for bucket operations." });
                }

                // Initialize Supabase storage
                _logger.LogInformation("Initializing Supabase client...");
                await _supabaseClient.InitializeAsync();
                var storage = _supabaseClient.Storage;
                var bucketName = _configuration["Supabase:Storage:BucketName"] ?? "media";
                _logger.LogInformation("Supabase client initialized. Target bucket: {BucketName}", bucketName);

                // Check if bucket exists, create if not
                try
                {
                    var buckets = await storage.ListBuckets();
                    var bucket = buckets.FirstOrDefault(b => b.Name == bucketName);
                    
                    if (bucket == null)
                    {
                        _logger.LogInformation("Creating bucket: {BucketName}", bucketName);
                        try
                        {
                            // Try with minimal options first
                            await storage.CreateBucket(bucketName, new Supabase.Storage.BucketUpsertOptions
                            {
                                Public = true
                            });
                            _logger.LogInformation("Bucket created successfully with minimal options: {BucketName}", bucketName);
                        }
                        catch (Exception bucketEx)
                        {
                            _logger.LogWarning("Failed to create bucket with minimal options: {Error}", bucketEx.Message);
                            
                            // Try with more specific options
                            try
                            {
                                await storage.CreateBucket(bucketName, new Supabase.Storage.BucketUpsertOptions
                                {
                                    Public = true,
                                    FileSizeLimit = "50mb",
                                    AllowedMimes = new List<string> { "image/*", "video/*", "audio/*" }
                                });
                                _logger.LogInformation("Bucket created successfully with specific options: {BucketName}", bucketName);
                            }
                            catch (Exception bucketEx2)
                            {
                                _logger.LogError("Failed to create bucket: {Error}", bucketEx2.Message);
                                return StatusCode(500, new { Message = $"Failed to create storage bucket: {bucketEx2.Message}" });
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Bucket already exists: {BucketName}", bucketName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Bucket check/create failed: {Error}", ex.Message);
                    return StatusCode(500, new { Message = $"Failed to access storage bucket: {ex.Message}" });
                }

                // Upload file with better error handling
                using var stream = file.OpenReadStream();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                try
                {
                    var result = await storage
                        .From(bucketName)
                        .Upload(fileBytes, filePath, new Supabase.Storage.FileOptions
                        {
                            CacheControl = "3600",
                            Upsert = true, // Allow overwrite
                            ContentType = GetContentType(Path.GetExtension(file.FileName))
                        });

                    if (result == null)
                    {
                        return StatusCode(500, new { Message = "Failed to upload file to storage. Result is null." });
                    }

                    _logger.LogInformation("File uploaded successfully to Supabase: {FilePath}", filePath);
                }
                catch (Exception uploadEx)
                {
                    _logger.LogError(uploadEx, "Supabase upload failed: {Error}", uploadEx.Message);
                    
                    // Return more specific error message
                    if (uploadEx.Message.Contains("bucket"))
                    {
                        return StatusCode(500, new { Message = "Storage bucket not found. Please check your Supabase configuration." });
                    }
                    else if (uploadEx.Message.Contains("permission"))
                    {
                        return StatusCode(500, new { Message = "Permission denied. Please check your Supabase API keys." });
                    }
                    else if (uploadEx.Message.Contains("JSON"))
                    {
                        return StatusCode(500, new { Message = "Supabase configuration error. Please check your Supabase URL and API keys in appsettings.json." });
                    }
                    else
                    {
                        return StatusCode(500, new { Message = $"Upload failed: {uploadEx.Message}" });
                    }
                }

                // Continue with success response

                // Get public URL
                var publicUrl = storage
                    .From(bucketName)
                    .GetPublicUrl(filePath);

                _logger.LogInformation("File uploaded successfully: {FilePath} by user {UserId}", filePath, userId);

                var fileType = GetFileType(fileExtension);

                var response = new FileUploadResponse
                {
                    FileName = fileName,
                    FilePath = filePath,
                    PublicUrl = publicUrl,
                    FileSize = file.Length,
                    ContentType = file.ContentType,
                    UploadedAt = DateTime.UtcNow,
                    UserId = userId,
                    FileType = fileType
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, new { Message = "An error occurred while uploading the file." });
            }
        }

        /// <summary>
        /// Download a file from Supabase storage
        /// </summary>
        /// <param name="filePath">Path to the file in storage</param>
        /// <returns>File download</returns>
        [HttpGet("media/download")]
        [SwaggerOperation(Summary = "Download file", Description = "Downloads a file from Supabase storage")]
        [SwaggerResponse(200, "File downloaded successfully")]
        [SwaggerResponse(404, "File not found", typeof(object))]
        [SwaggerResponse(500, "Download failed", typeof(object))]
        public async Task<IActionResult> DownloadFile([FromQuery] string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return BadRequest(new { Message = "File path is required." });
                }

                // Initialize Supabase storage
                await _supabaseClient.InitializeAsync();
                var storage = _supabaseClient.Storage;
                var bucketName = _configuration["Supabase:Storage:BucketName"] ?? "media";

                // Download file
                var fileBytes = await storage
                    .From(bucketName)
                    .Download(filePath, null);

                if (fileBytes == null || fileBytes.Length == 0)
                {
                    return NotFound(new { Message = "File not found." });
                }

                // Get file info
                var fileName = Path.GetFileName(filePath);
                var contentType = GetContentType(Path.GetExtension(filePath));

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file: {FilePath}", filePath);
                return StatusCode(500, new { Message = "An error occurred while downloading the file." });
            }
        }

        /// <summary>
        /// Get public URL for a file
        /// </summary>
        /// <param name="filePath">Path to the file in storage</param>
        /// <returns>Public URL</returns>
        [HttpGet("media/url")]
        [SwaggerOperation(Summary = "Get file URL", Description = "Gets public URL for a file in storage")]
        [SwaggerResponse(200, "URL retrieved successfully", typeof(FileUrlResponse))]
        [SwaggerResponse(400, "Invalid request", typeof(object))]
        public async Task<IActionResult> GetFileUrl([FromQuery] string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return BadRequest(new { Message = "File path is required." });
                }

                // Initialize Supabase storage
                await _supabaseClient.InitializeAsync();
                var storage = _supabaseClient.Storage;
                var bucketName = _configuration["Supabase:Storage:BucketName"] ?? "media";

                // Get public URL
                var publicUrl = storage
                    .From(bucketName)
                    .GetPublicUrl(filePath);

                var response = new FileUrlResponse
                {
                    FilePath = filePath,
                    PublicUrl = publicUrl
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file URL: {FilePath}", filePath);
                return StatusCode(500, new { Message = "An error occurred while getting the file URL." });
            }
        }

        /// <summary>
        /// Delete a file from Supabase storage
        /// </summary>
        /// <param name="filePath">Path to the file in storage</param>
        /// <returns>Delete result</returns>
        [HttpDelete("media/delete")]
        [SwaggerOperation(Summary = "Delete file", Description = "Deletes a file from Supabase storage")]
        [SwaggerResponse(200, "File deleted successfully", typeof(object))]
        [SwaggerResponse(404, "File not found", typeof(object))]
        [SwaggerResponse(500, "Delete failed", typeof(object))]
        public async Task<IActionResult> DeleteFile([FromQuery] string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return BadRequest(new { Message = "File path is required." });
                }

                // Get user ID from JWT token
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "User not authenticated." });
                }

                // Check if user owns the file (basic security check)
                if (!filePath.Contains($"/{userId}/"))
                {
                    return Forbid("You can only delete your own files.");
                }

                // Initialize Supabase storage
                await _supabaseClient.InitializeAsync();
                var storage = _supabaseClient.Storage;
                var bucketName = _configuration["Supabase:Storage:BucketName"] ?? "media";

                // Delete file
                var result = await storage
                    .From(bucketName)
                    .Remove(filePath);

                _logger.LogInformation("File deleted successfully: {FilePath} by user {UserId}", filePath, userId);

                return Ok(new { Message = "File deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
                return StatusCode(500, new { Message = "An error occurred while deleting the file." });
            }
        }

        /// <summary>
        /// List files in a folder
        /// </summary>
        /// <param name="folder">Folder path (default: "uploads")</param>
        /// <returns>List of files</returns>
        [HttpGet("media/list")]
        [SwaggerOperation(Summary = "List files", Description = "Lists files in a specified folder")]
        [SwaggerResponse(200, "Files listed successfully", typeof(FileListResponse))]
        [SwaggerResponse(500, "List operation failed", typeof(object))]
        public async Task<IActionResult> ListFiles([FromQuery] string folder = "uploads")
        {
            try
            {
                // Get user ID from JWT token
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "User not authenticated." });
                }

                // Initialize Supabase storage
                await _supabaseClient.InitializeAsync();
                var storage = _supabaseClient.Storage;
                var bucketName = _configuration["Supabase:Storage:BucketName"] ?? "media";

                // List files in user's folder
                var userFolder = $"{folder}/{userId}";
                var files = await storage
                    .From(bucketName)
                    .List(userFolder);

                var fileInfos = files?.Select(f => 
                {
                    long fileSize = 0;
                    if (f.MetaData != null && f.MetaData.TryGetValue("size", out var sizeObj))
                    {
                        if (sizeObj is long size)
                            fileSize = size;
                        else if (sizeObj is int intSize)
                            fileSize = intSize;
                    }

                    var fileExtension = Path.GetExtension(f.Name).ToLower();
                    var fileType = GetFileType(fileExtension);

                    return new FileInfo
                    {
                        Name = f.Name,
                        Path = $"{userFolder}/{f.Name}",
                        Size = fileSize,
                        LastModified = f.UpdatedAt ?? DateTime.MinValue,
                        PublicUrl = storage.From(bucketName).GetPublicUrl($"{userFolder}/{f.Name}"),
                        FileType = fileType,
                        FileExtension = fileExtension
                    };
                })
                .ToList() ?? new List<FileInfo>();

                var response = new FileListResponse
                {
                    Folder = userFolder,
                    Files = fileInfos,
                    TotalCount = fileInfos.Count
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files in folder: {Folder}", folder);
                return StatusCode(500, new { Message = "An error occurred while listing files." });
            }
        }

        private string GetFileType(string fileExtension)
        {
            return fileExtension.ToLower() switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp" or ".svg" => "Image",
                ".mp4" or ".avi" or ".mov" or ".wmv" or ".webm" or ".mkv" or ".flv" => "Video",
                ".mp3" or ".wav" or ".flac" or ".aac" or ".ogg" => "Audio",
                ".pdf" => "PDF",
                ".docx" or ".doc" => "Word Document",
                ".xlsx" or ".xls" => "Excel Document",
                ".pptx" or ".ppt" => "PowerPoint Document",
                ".txt" => "Text Document",
                ".zip" or ".rar" or ".7z" => "Archive",
                _ => "File"
            };
        }

        private string GetContentType(string fileExtension)
        {
            return fileExtension.ToLower() switch
            {
                // Images
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                ".svg" => "image/svg+xml",
                
                // Videos
                ".mp4" => "video/mp4",
                ".avi" => "video/avi",
                ".mov" => "video/quicktime",
                ".wmv" => "video/x-ms-wmv",
                ".webm" => "video/webm",
                ".mkv" => "video/x-matroska",
                ".flv" => "video/x-flv",
                
                // Audio
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".flac" => "audio/flac",
                ".aac" => "audio/aac",
                ".ogg" => "audio/ogg",
                
                // Documents
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc" => "application/msword",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".xls" => "application/vnd.ms-excel",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".txt" => "text/plain",
                
                // Archives
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                ".7z" => "application/x-7z-compressed",
                
                _ => "application/octet-stream"
            };
        }
    }

    // Response models
    public class FileUploadResponse
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string PublicUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
    }

    public class FileUrlResponse
    {
        public string FilePath { get; set; } = string.Empty;
        public string PublicUrl { get; set; } = string.Empty;
    }

    public class FileListResponse
    {
        public string Folder { get; set; } = string.Empty;
        public List<FileInfo> Files { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class FileInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public string PublicUrl { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
    }
}
