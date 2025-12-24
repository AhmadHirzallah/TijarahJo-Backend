using System.ComponentModel.DataAnnotations;

namespace TijarahJoDBAPI.DTOs.Requests;

public class CreatePostImageRequest
{
    [Required(ErrorMessage = "PostID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "PostID must be a positive value.")]
    public int PostID { get; set; }

    [Required(ErrorMessage = "Post image URL is required.")]
    [Url(ErrorMessage = "Invalid URL format.")]
    [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters.")]
    public string PostImageURL { get; set; } = string.Empty;
}
