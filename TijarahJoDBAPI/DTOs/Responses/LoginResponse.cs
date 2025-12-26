namespace TijarahJoDBAPI.DTOs.Responses;

/// <summary>
/// Response returned after successful login
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// User information (without sensitive data)
    /// </summary>
    public UserResponse User { get; set; } = null!;

    /// <summary>
    /// JWT access token for authentication
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time in UTC
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// User's role for frontend authorization
    /// </summary>
    public string Role { get; set; } = string.Empty;
}
