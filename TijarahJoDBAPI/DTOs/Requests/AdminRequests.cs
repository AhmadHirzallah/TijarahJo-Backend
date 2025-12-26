using System.ComponentModel.DataAnnotations;

namespace TijarahJoDBAPI.DTOs.Requests;

/// <summary>
/// Request to update a user's role
/// </summary>
public class UpdateUserRoleRequest
{
    /// <summary>
    /// New role ID: 1=Admin, 2=Moderator, 3+=User
    /// </summary>
    [Required(ErrorMessage = "Role ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Role ID must be a positive integer.")]
    public int RoleID { get; set; }
}

/// <summary>
/// Request to update a user's status
/// </summary>
public class UpdateUserStatusRequest
{
    /// <summary>
    /// New status: 0=Active, 1=Inactive, 2=Banned, 3=Suspended
    /// </summary>
    [Required(ErrorMessage = "Status is required.")]
    [Range(0, 3, ErrorMessage = "Status must be between 0 and 3.")]
    public int Status { get; set; }

    /// <summary>
    /// Optional reason for the status change
    /// </summary>
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
    public string? Reason { get; set; }
}

/// <summary>
/// Request to update a post's status
/// </summary>
public class UpdatePostStatusRequest
{
    /// <summary>
    /// New status: 0=Draft, 1=PendingReview, 2=Active, 3=Sold, 4=Expired, 5=Rejected, 6=Removed
    /// </summary>
    [Required(ErrorMessage = "Status is required.")]
    [Range(0, 7, ErrorMessage = "Status must be between 0 and 7.")]
    public int Status { get; set; }

    /// <summary>
    /// Optional reason for the status change (required for reject/remove)
    /// </summary>
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
    public string? Reason { get; set; }
}
