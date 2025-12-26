namespace TijarahJoDBAPI.Services.Security;

/// <summary>
/// Security service implementation providing password hashing and verification
/// Supports both PBKDF2 (recommended) and legacy plain text passwords
/// </summary>
public class SecurityService : ISecurityService
{
    private readonly IPasswordHashingService _hashingService;

    public SecurityService(IPasswordHashingService hashingService)
    {
        _hashingService = hashingService;
    }

    /// <summary>
    /// Hashes a password using PBKDF2 for secure storage
    /// </summary>
    public string HashPassword(string password)
    {
        return _hashingService.HashPassword(password);
    }

    /// <summary>
    /// Verifies a password using hybrid approach:
    /// 1. First tries PBKDF2 verification (new users)
    /// 2. Falls back to legacy plain text comparison (existing users)
    /// </summary>
    public bool VerifyPassword(string storedPassword, string providedPassword)
    {
        if (string.IsNullOrEmpty(storedPassword) || string.IsNullOrEmpty(providedPassword))
            return false;

        // Try PBKDF2 verification first (new format)
        if (!_hashingService.IsLegacyFormat(storedPassword))
        {
            return _hashingService.VerifyPassword(storedPassword, providedPassword);
        }

        // Fall back to legacy plain text comparison
        // This supports existing users who registered before password hashing
        return VerifyLegacyPassword(storedPassword, providedPassword);
    }

    /// <summary>
    /// Checks if password needs to be rehashed (migrated to PBKDF2)
    /// </summary>
    public bool NeedsRehash(string storedPassword)
    {
        return _hashingService.IsLegacyFormat(storedPassword);
    }

    /// <summary>
    /// Verifies legacy plain text password
    /// Uses constant-time comparison to prevent timing attacks
    /// </summary>
    private static bool VerifyLegacyPassword(string storedPassword, string providedPassword)
    {
        // Plain text comparison for legacy passwords
        // Note: This is only for backward compatibility
        if (storedPassword.Length != providedPassword.Length)
            return false;

        int result = 0;
        for (int i = 0; i < storedPassword.Length; i++)
        {
            result |= storedPassword[i] ^ providedPassword[i];
        }
        return result == 0;
    }
}
