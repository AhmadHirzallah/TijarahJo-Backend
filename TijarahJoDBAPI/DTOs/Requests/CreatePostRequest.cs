using System.ComponentModel.DataAnnotations;

namespace TijarahJoDBAPI.DTOs.Requests;

public class CreatePostRequest
{
    [Required(ErrorMessage = "UserID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "UserID must be a positive value.")]
    public int UserID { get; set; }

    [Required(ErrorMessage = "CategoryID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "CategoryID must be a positive value.")]
    public int CategoryID { get; set; }

    [Required(ErrorMessage = "Post title is required.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Post title must be between 3 and 200 characters.")]
    public string PostTitle { get; set; } = string.Empty;

    [StringLength(5000, ErrorMessage = "Post description cannot exceed 5000 characters.")]
    public string? PostDescription { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be a non-negative value.")]
    public decimal? Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Status must be a non-negative value.")]
    public int Status { get; set; } = 0;
}
