using System.ComponentModel.DataAnnotations;

namespace TijarahJoDBAPI.DTOs.Requests;

/// <summary>
/// Request DTO for creating a post image from URL
/// Note: PostID comes from route parameter
/// </summary>
public class CreatePostImageRequest
{
    /// <summary>
    /// URL of the image (if already uploaded elsewhere)
    /// </summary>
    [Required(ErrorMessage = "Post image URL is required.")]
    [Url(ErrorMessage = "Invalid URL format.")]
    [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters.")]
    public string PostImageURL { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for uploading a post image via Base64
/// </summary>
public class UploadPostImageBase64Request
{
    /// <summary>
    /// Base64 encoded image data (with or without data URI prefix)
    /// Example: "data:image/png;base64,iVBORw0KGgo..." or just "iVBORw0KGgo..."
    /// </summary>
    [Required(ErrorMessage = "Image data is required.")]
    public string ImageData { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for updating an existing post image
/// </summary>
public class UpdatePostImageRequest
{
    /// <summary>
    /// Updated image URL
    /// </summary>
    [Required(ErrorMessage = "Post image URL is required.")]
    [Url(ErrorMessage = "Invalid URL format.")]
    [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters.")]
    public string PostImageURL { get; set; } = string.Empty;

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}
