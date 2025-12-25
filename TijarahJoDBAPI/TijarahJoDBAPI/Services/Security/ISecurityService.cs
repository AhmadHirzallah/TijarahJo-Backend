namespace TijarahJoDBAPI.Services.Security;

/// <summary>
/// Security service interface providing unified access to password operations
/// Acts as a facade over different security implementations
/// </summary>
public interface ISecurityService
{
    /// <summary>
    /// Hashes a password for storage (uses PBKDF2)
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password for database storage</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a stored hash
    /// Supports both PBKDF2 (new) and legacy formats
    /// </summary>
    /// <param name="storedPassword">Password from database</param>
    /// <param name="providedPassword">Password to verify</param>
    /// <returns>True if password is valid</returns>
    bool VerifyPassword(string storedPassword, string providedPassword);

    /// <summary>
    /// Checks if password needs rehashing (legacy format)
    /// </summary>
    /// <param name="storedPassword">Password from database</param>
    /// <returns>True if password should be rehashed</returns>
    bool NeedsRehash(string storedPassword);
}
