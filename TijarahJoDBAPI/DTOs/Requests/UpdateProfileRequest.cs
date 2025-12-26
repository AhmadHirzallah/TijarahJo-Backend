using System.ComponentModel.DataAnnotations;

namespace TijarahJoDBAPI.DTOs.Requests;

/// <summary>
/// Request model for users to update their own profile.
/// This is a simplified version - users can update basic info without password.
/// </summary>
public class UpdateProfileRequest
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
    public string? Username { get; set; }
}
