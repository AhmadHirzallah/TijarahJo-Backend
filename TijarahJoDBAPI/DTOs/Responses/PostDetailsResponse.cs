namespace TijarahJoDBAPI.DTOs.Responses;

/// <summary>
/// Complete post details response including owner, category, reviews, and images
/// </summary>
public class PostDetailsResponse
{
    #region Post Info

    public int PostID { get; set; }
    public string PostTitle { get; set; } = string.Empty;
    public string? PostDescription { get; set; }
    public decimal? Price { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }

    #endregion

    #region Owner Info

    public int OwnerUserID { get; set; }
    public string? OwnerUsername { get; set; }
    public string? OwnerEmail { get; set; }
    public string? OwnerFirstName { get; set; }
    public string? OwnerLastName { get; set; }
    public string? OwnerFullName { get; set; }

    /// <summary>
    /// Owner's primary phone number for WhatsApp contact
    /// </summary>
    public string? OwnerPrimaryPhone { get; set; }

    #endregion

    #region Role Info

    public int? RoleID { get; set; }
    public string? RoleName { get; set; }

    #endregion

    #region Category Info

    public int CategoryID { get; set; }
    public string? CategoryName { get; set; }

    #endregion

    #region Related Data

    /// <summary>
    /// List of reviews for this post
    /// </summary>
    public List<ReviewDetailResponse> Reviews { get; set; } = new();

    /// <summary>
    /// List of images for this post
    /// </summary>
    public List<PostImageDetailResponse> Images { get; set; } = new();

    #endregion

    #region Computed Properties

    /// <summary>
    /// Total number of reviews
    /// </summary>
    public int ReviewCount => Reviews.Count;

    /// <summary>
    /// Average rating (null if no reviews)
    /// </summary>
    public double? AverageRating => Reviews.Count > 0 
        ? Math.Round(Reviews.Average(r => r.Rating), 1) 
        : null;

    /// <summary>
    /// Total number of images
    /// </summary>
    public int ImageCount => Images.Count;

    /// <summary>
    /// Primary image URL (first image)
    /// </summary>
    public string? PrimaryImageUrl => Images.FirstOrDefault()?.PostImageURL;

    /// <summary>
    /// Indicates if the owner has a phone number available for contact
    /// </summary>
    public bool HasOwnerPhone => !string.IsNullOrEmpty(OwnerPrimaryPhone);

    /// <summary>
    /// WhatsApp deep link URL for contacting the owner
    /// Format: https://wa.me/{phoneNumber}
    /// Returns null if no phone number available
    /// </summary>
    public string? WhatsAppLink => HasOwnerPhone 
        ? $"https://wa.me/{OwnerPrimaryPhone?.Replace("+", "").Replace(" ", "").Replace("-", "")}" 
        : null;

    #endregion
}

/// <summary>
/// Image details response (from SP_GetPostDetails_All)
/// </summary>
public class PostImageDetailResponse
{
    public int PostImageID { get; set; }
    public int PostID { get; set; }
    public string PostImageURL { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
