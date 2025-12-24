using System.ComponentModel.DataAnnotations;

namespace TijarahJoDBAPI.DTOs.Requests;

public class LoginRequest
{
    /// <summary>
    /// Username or Email for login
    /// </summary>
    [Required(ErrorMessage = "Login is required.")]
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// User's password (will be hashed for comparison)
    /// </summary>
    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}
