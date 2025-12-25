namespace TijarahJoDBAPI.DTOs.Responses;

/// <summary>
/// Response DTO for a post image
/// </summary>
public class PostImageResponse
{
    public int? PostImageID { get; set; }
    public int PostID { get; set; }
    public string PostImageURL { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public bool IsDeleted { get; set; }
}

/// <summary>
/// Response containing all images for a post
/// </summary>
public class PostImagesResponse
{
    /// <summary>
    /// The post ID these images belong to
    /// </summary>
    public int PostID { get; set; }

    /// <summary>
    /// List of post images
    /// </summary>
    public List<PostImageResponse> Images { get; set; } = new();

    /// <summary>
    /// Total number of images
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Primary/first image URL
    /// </summary>
    public string? PrimaryImageUrl => Images.FirstOrDefault()?.PostImageURL;
}

/// <summary>
/// Response after uploading a post image
/// </summary>
public class PostImageUploadResponse
{
    /// <summary>
    /// Whether the upload was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The database record of the uploaded image
    /// </summary>
    public PostImageResponse? Image { get; set; }

    /// <summary>
    /// Error message if upload failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; set; }
}
