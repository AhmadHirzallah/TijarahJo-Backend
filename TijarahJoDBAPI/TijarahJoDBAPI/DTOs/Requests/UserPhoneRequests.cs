using System.ComponentModel.DataAnnotations;

namespace TijarahJoDBAPI.DTOs.Requests;

/// <summary>
/// Request to create a new phone number for a user
/// </summary>
public class CreateUserPhoneRequest
{
    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Set to true to make this the primary/main phone number
    /// </summary>
    public bool IsPrimary { get; set; } = false;
}

/// <summary>
/// Request to update an existing phone number
/// </summary>
public class UpdateUserPhoneRequest
{
    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Set to true to make this the primary/main phone number
    /// </summary>
    public bool IsPrimary { get; set; } = false;

    public bool IsDeleted { get; set; } = false;
}
