using System.ComponentModel.DataAnnotations;

namespace TijarahJoDBAPI.DTOs.Requests;

/// <summary>
/// Request DTO for uploading a user image via URL
/// </summary>
public class CreateUserImageRequest
{
    /// <summary>
    /// URL of the image (if already uploaded elsewhere)
    /// </summary>
    [Required(ErrorMessage = "Image URL is required.")]
    [Url(ErrorMessage = "Invalid URL format.")]
    [StringLength(1000, ErrorMessage = "URL cannot exceed 1000 characters.")]
    public string ImageURL { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for uploading a user image via Base64
/// </summary>
public class UploadUserImageBase64Request
{
    /// <summary>
    /// Base64 encoded image data (with or without data URI prefix)
    /// Example: "data:image/png;base64,iVBORw0KGgo..." or just "iVBORw0KGgo..."
    /// </summary>
    [Required(ErrorMessage = "Image data is required.")]
    public string ImageData { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for updating a user image
/// </summary>
public class UpdateUserImageRequest
{
    /// <summary>
    /// Updated image URL
    /// </summary>
    [Required(ErrorMessage = "Image URL is required.")]
    [Url(ErrorMessage = "Invalid URL format.")]
    [StringLength(1000, ErrorMessage = "URL cannot exceed 1000 characters.")]
    public string ImageURL { get; set; } = string.Empty;

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}
