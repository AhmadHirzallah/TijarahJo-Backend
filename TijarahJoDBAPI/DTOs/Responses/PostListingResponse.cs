namespace TijarahJoDBAPI.DTOs.Responses;

/// <summary>
/// Response DTO for paginated post listing with enriched user and category data
/// </summary>
public class PostListingResponse
{
    #region Post Info

    public int? PostID { get; set; }
    public string PostTitle { get; set; } = string.Empty;
    public string? PostDescription { get; set; }
    public decimal? Price { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }

    #endregion

    #region User Info (from join)

    public int UserID { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    /// <summary>
    /// Computed full name of the seller
    /// </summary>
    public string SellerFullName => string.IsNullOrEmpty(LastName)
        ? FirstName ?? string.Empty
        : $"{FirstName} {LastName}";

    #endregion

    #region Role Info (from join)

    public int? RoleID { get; set; }
    public string? RoleName { get; set; }

    #endregion

    #region Category Info (from join)

    public int CategoryID { get; set; }
    public string? CategoryName { get; set; }

    #endregion

    #region Image Info

    /// <summary>
    /// Primary image URL for the post (first uploaded image)
    /// </summary>
    public string? PrimaryImageUrl { get; set; }

    #endregion
}
