namespace TijarahJoDBAPI.DTOs.Responses;

/// <summary>
/// Basic review response DTO
/// </summary>
public class ReviewResponse
{
    public int? ReviewID { get; set; }
    public int PostID { get; set; }
    public int UserID { get; set; }
    public byte Rating { get; set; }
    public string? ReviewText { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}

/// <summary>
/// Enriched review response with reviewer details (from SP_GetPostDetails_All)
/// </summary>
public class ReviewDetailResponse
{
    #region Review Info

    public int ReviewID { get; set; }
    public int PostID { get; set; }
    public byte Rating { get; set; }
    public string? ReviewText { get; set; }
    public DateTime CreatedAt { get; set; }

    #endregion

    #region Reviewer Info

    public int ReviewerUserID { get; set; }
    public string? ReviewerUsername { get; set; }
    public string? ReviewerEmail { get; set; }
    public string? ReviewerFirstName { get; set; }
    public string? ReviewerLastName { get; set; }
    public string? ReviewerFullName { get; set; }

    #endregion
}

/// <summary>
/// Response containing all reviews for a post with summary statistics
/// </summary>
public class PostReviewsResponse
{
    /// <summary>
    /// The post ID these reviews belong to
    /// </summary>
    public int PostID { get; set; }

    /// <summary>
    /// List of reviews for the post
    /// </summary>
    public List<ReviewResponse> Reviews { get; set; } = new();

    /// <summary>
    /// Total number of reviews
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Average rating (null if no reviews)
    /// </summary>
    public double? AverageRating { get; set; }

    /// <summary>
    /// Rating distribution (count per star)
    /// </summary>
    public Dictionary<int, int> RatingDistribution => Reviews
        .GroupBy(r => r.Rating)
        .ToDictionary(g => (int)g.Key, g => g.Count());
}
