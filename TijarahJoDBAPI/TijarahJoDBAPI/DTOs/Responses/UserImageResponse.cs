namespace TijarahJoDBAPI.DTOs.Responses;

/// <summary>
/// Response DTO for a user image
/// </summary>
public class UserImageResponse
{
    public int? UserImageID { get; set; }
    public int UserID { get; set; }
    public string ImageURL { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public bool IsDeleted { get; set; }
}

/// <summary>
/// Response containing all images for a user
/// </summary>
public class UserImagesResponse
{
    /// <summary>
    /// The user ID these images belong to
    /// </summary>
    public int UserID { get; set; }

    /// <summary>
    /// List of user images
    /// </summary>
    public List<UserImageResponse> Images { get; set; } = new();

    /// <summary>
    /// Total number of images
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Primary/first image URL (for profile display)
    /// </summary>
    public string? PrimaryImageUrl => Images.FirstOrDefault()?.ImageURL;
}

/// <summary>
/// Response after uploading an image
/// </summary>
public class ImageUploadResponse
{
    /// <summary>
    /// Whether the upload was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The database record of the uploaded image
    /// </summary>
    public UserImageResponse? Image { get; set; }

    /// <summary>
    /// Error message if upload failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; set; }
}
