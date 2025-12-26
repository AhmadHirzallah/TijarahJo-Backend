using System.ComponentModel.DataAnnotations;

namespace TijarahJoDBAPI.DTOs.Requests;

/// <summary>
/// Request DTO for creating a new review.
/// PostID is derived from the route parameter /api/posts/{postId}/reviews
/// </summary>
public class CreateReviewRequest
{
    /// <summary>
    /// The user submitting the review
    /// </summary>
    [Required(ErrorMessage = "UserID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "UserID must be a positive value.")]
    public int UserID { get; set; }

    /// <summary>
    /// Rating from 1 to 5
    /// </summary>
    [Required(ErrorMessage = "Rating is required.")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public byte Rating { get; set; }

    /// <summary>
    /// Optional review text/comment
    /// </summary>
    [StringLength(1000, ErrorMessage = "Review text cannot exceed 1000 characters.")]
    public string? ReviewText { get; set; }
}
