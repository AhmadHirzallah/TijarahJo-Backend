using Microsoft.AspNetCore.Http;

namespace TijarahJoDBAPI.Services;

/// <summary>
/// Result of an image upload operation
/// </summary>
public class ImageUploadResult
{
    public bool Success { get; set; }
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public long FileSizeBytes { get; set; }
}

/// <summary>
/// Service for handling image uploads for users and posts
/// </summary>
public class ImageUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Allowed image extensions
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    // Max file size: 5MB
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    public ImageUploadService(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
    {
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Uploads an image file for a user
    /// </summary>
    /// <param name="file">The image file</param>
    /// <param name="userId">The user ID (for folder organization)</param>
    /// <returns>Upload result with URL</returns>
    public async Task<ImageUploadResult> UploadUserImageAsync(IFormFile file, int userId)
    {
        return await UploadImageAsync(file, "users", userId.ToString());
    }

    /// <summary>
    /// Uploads an image file for a post
    /// </summary>
    /// <param name="file">The image file</param>
    /// <param name="postId">The post ID (for folder organization)</param>
    /// <returns>Upload result with URL</returns>
    public async Task<ImageUploadResult> UploadPostImageAsync(IFormFile file, int postId)
    {
        return await UploadImageAsync(file, "posts", postId.ToString());
    }

    /// <summary>
    /// Uploads an image from Base64 string for a user
    /// </summary>
    public async Task<ImageUploadResult> UploadUserImageFromBase64Async(string base64Data, int userId)
    {
        return await UploadImageFromBase64Async(base64Data, "users", userId.ToString());
    }

    /// <summary>
    /// Uploads an image from Base64 string for a post
    /// </summary>
    public async Task<ImageUploadResult> UploadPostImageFromBase64Async(string base64Data, int postId)
    {
        return await UploadImageFromBase64Async(base64Data, "posts", postId.ToString());
    }

    /// <summary>
    /// Core upload logic for IFormFile
    /// </summary>
    private async Task<ImageUploadResult> UploadImageAsync(IFormFile file, string category, string entityId)
    {
        try
        {
            // Validate file
            if (file == null || file.Length == 0)
            {
                return new ImageUploadResult
                {
                    Success = false,
                    ErrorMessage = "No file provided or file is empty."
                };
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return new ImageUploadResult
                {
                    Success = false,
                    ErrorMessage = $"File size exceeds maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)}MB."
                };
            }

            // Get and validate extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return new ImageUploadResult
                {
                    Success = false,
                    ErrorMessage = $"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}"
                };
            }

            // Generate unique filename: {entityId}_{guid}_{timestamp}.{ext}
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var fileName = $"{entityId}_{Guid.NewGuid():N}_{timestamp}{extension}";

            // Create folder path: wwwroot/uploads/{category}/{entityId}/
            var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folderPath = Path.Combine(webRootPath, "uploads", category, entityId);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);

            // Save file
            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Build public URL
            var request = _httpContextAccessor.HttpContext?.Request;
            var fileUrl = $"{request?.Scheme}://{request?.Host}/uploads/{category}/{entityId}/{fileName}";

            return new ImageUploadResult
            {
                Success = true,
                FileName = fileName,
                FileUrl = fileUrl,
                FileSizeBytes = file.Length
            };
        }
        catch (Exception ex)
        {
            return new ImageUploadResult
            {
                Success = false,
                ErrorMessage = $"Upload failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Core upload logic for Base64 string
    /// </summary>
    private async Task<ImageUploadResult> UploadImageFromBase64Async(string base64Data, string category, string entityId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(base64Data))
            {
                return new ImageUploadResult
                {
                    Success = false,
                    ErrorMessage = "No image data provided."
                };
            }

            // Parse Base64 data and determine extension
            string extension = ".jpg"; // Default
            string actualBase64 = base64Data;

            if (base64Data.Contains(","))
            {
                var parts = base64Data.Split(',');
                var header = parts[0].ToLowerInvariant();

                if (header.Contains("png")) extension = ".png";
                else if (header.Contains("gif")) extension = ".gif";
                else if (header.Contains("webp")) extension = ".webp";
                else if (header.Contains("jpeg") || header.Contains("jpg")) extension = ".jpg";

                actualBase64 = parts[1];
            }

            // Decode and validate size
            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(actualBase64);
            }
            catch (FormatException)
            {
                return new ImageUploadResult
                {
                    Success = false,
                    ErrorMessage = "Invalid Base64 image data."
                };
            }

            if (imageBytes.Length > MaxFileSizeBytes)
            {
                return new ImageUploadResult
                {
                    Success = false,
                    ErrorMessage = $"Image size exceeds maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)}MB."
                };
            }

            // Generate unique filename
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var fileName = $"{entityId}_{Guid.NewGuid():N}_{timestamp}{extension}";

            // Create folder path
            var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folderPath = Path.Combine(webRootPath, "uploads", category, entityId);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);

            // Save file
            await File.WriteAllBytesAsync(filePath, imageBytes);

            // Build public URL
            var request = _httpContextAccessor.HttpContext?.Request;
            var fileUrl = $"{request?.Scheme}://{request?.Host}/uploads/{category}/{entityId}/{fileName}";

            return new ImageUploadResult
            {
                Success = true,
                FileName = fileName,
                FileUrl = fileUrl,
                FileSizeBytes = imageBytes.Length
            };
        }
        catch (Exception ex)
        {
            return new ImageUploadResult
            {
                Success = false,
                ErrorMessage = $"Upload failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Deletes an image file from the server
    /// </summary>
    /// <param name="imageUrl">The full URL of the image</param>
    /// <returns>True if deleted successfully</returns>
    public bool DeleteImage(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
                return false;

            // Extract relative path from URL
            // URL format: https://host/uploads/category/entityId/filename.ext
            var uri = new Uri(imageUrl);
            var relativePath = uri.AbsolutePath.TrimStart('/');

            var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var filePath = Path.Combine(webRootPath, relativePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
