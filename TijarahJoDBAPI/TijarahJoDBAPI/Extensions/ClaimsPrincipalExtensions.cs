using System.Security.Claims;

namespace TijarahJoDBAPI.Extensions;

/// <summary>
/// Extension methods for accessing JWT claims from ClaimsPrincipal
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the user ID from JWT claims
    /// </summary>
    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// Gets the user ID from JWT claims (throws if not found)
    /// </summary>
    public static int GetUserIdRequired(this ClaimsPrincipal user)
    {
        var userId = user.GetUserId();
        return userId ?? throw new UnauthorizedAccessException("User ID not found in token");
    }

    /// <summary>
    /// Gets the role from JWT claims
    /// </summary>
    public static string? GetRole(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Role)?.Value;
    }

    /// <summary>
    /// Gets the username from JWT claims
    /// </summary>
    public static string? GetUsername(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Checks if the user has Admin role
    /// </summary>
    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.IsInRole(RoleNames.Admin);
    }

    /// <summary>
    /// Checks if the current user matches the specified user ID or is an admin
    /// </summary>
    public static bool IsOwnerOrAdmin(this ClaimsPrincipal user, int resourceOwnerId)
    {
        var currentUserId = user.GetUserId();
        return currentUserId == resourceOwnerId || user.IsAdmin();
    }
}

/// <summary>
/// Constants for role names - keeps them consistent across the application
/// </summary>
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Moderator = "Moderator";
    
    // Combined roles for [Authorize] attributes
    public const string AdminOrModerator = "Admin,Moderator";
}
