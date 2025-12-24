using System.ComponentModel.DataAnnotations;

namespace TijarahJoDBAPI.DTOs.Requests;

public class UpdateRoleRequest
{
    [Required(ErrorMessage = "Role name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 100 characters.")]
    public string RoleName { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false;
}
