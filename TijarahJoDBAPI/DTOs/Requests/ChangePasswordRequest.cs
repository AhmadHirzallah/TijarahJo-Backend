using System.ComponentModel.DataAnnotations;

namespace TijarahJoDBAPI.DTOs.Requests;

/// <summary>
/// Request to change user password
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// Current password for verification
    /// </summary>
    [Required(ErrorMessage = "Current password is required.")]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// New password to set
    /// </summary>
    [Required(ErrorMessage = "New password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Confirmation of new password
    /// </summary>
    [Required(ErrorMessage = "Password confirmation is required.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
