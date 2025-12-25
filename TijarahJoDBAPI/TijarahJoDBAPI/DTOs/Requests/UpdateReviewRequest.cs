using System.ComponentModel.DataAnnotations;

namespace TijarahJoDBAPI.DTOs.Requests;

/// <summary>
/// Request DTO for updating an existing review
/// </summary>
public class UpdateReviewRequest
{
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

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}
