namespace TijarahJoDBAPI.Services.Security;

/// <summary>
/// Interface for password hashing operations using PBKDF2
/// </summary>
public interface IPasswordHashingService
{
    /// <summary>
    /// Hashes a password using PBKDF2-HMAC-SHA256
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Base64 encoded hash containing salt and hash</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a stored hash
    /// </summary>
    /// <param name="hashedPassword">Stored hash from database</param>
    /// <param name="providedPassword">Password to verify</param>
    /// <returns>True if password matches</returns>
    bool VerifyPassword(string hashedPassword, string providedPassword);

    /// <summary>
    /// Checks if a stored password is using the legacy format
    /// </summary>
    /// <param name="storedPassword">Password from database</param>
    /// <returns>True if using legacy format</returns>
    bool IsLegacyFormat(string storedPassword);
}
