using System.Security.Cryptography;

namespace TijarahJoDBAPI.Services.Security;

/// <summary>
/// Password hashing service using PBKDF2-HMAC-SHA256
/// Implements OWASP recommended password storage guidelines
/// </summary>
public class PasswordHashingService : IPasswordHashingService
{
    // OWASP recommended configuration
    private const int SaltSize = 16;        // 128 bits
    private const int HashSize = 32;        // 256 bits
    private const int Iterations = 100_000; // OWASP minimum recommendation

    /// <summary>
    /// Hashes a password using PBKDF2-HMAC-SHA256 with random salt
    /// </summary>
    /// <param name="password">Plain text password to hash</param>
    /// <returns>Base64 encoded string containing [Salt][Hash]</returns>
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password), "Password cannot be null or empty");

        // Generate cryptographically secure random salt
        byte[] salt = GenerateSalt();

        // Hash password with salt using PBKDF2
        byte[] hash = HashPasswordWithSalt(password, salt);

        // Combine salt and hash: [Salt (16 bytes)][Hash (32 bytes)] = 48 bytes
        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        // Return as Base64 for database storage
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Verifies a password against a stored PBKDF2 hash
    /// Uses constant-time comparison to prevent timing attacks
    /// </summary>
    /// <param name="hashedPassword">Stored hash from database</param>
    /// <param name="providedPassword">Password to verify</param>
    /// <returns>True if password matches the stored hash</returns>
    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(providedPassword))
            return false;

        try
        {
            // Decode the stored Base64 hash
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);

            // Validate expected length (salt + hash)
            if (hashBytes.Length != SaltSize + HashSize)
                return false;

            // Extract salt (first 16 bytes)
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Extract stored hash (last 32 bytes)
            byte[] storedHash = new byte[HashSize];
            Array.Copy(hashBytes, SaltSize, storedHash, 0, HashSize);

            // Hash the provided password with the extracted salt
            byte[] providedHash = HashPasswordWithSalt(providedPassword, salt);

            // Constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(storedHash, providedHash);
        }
        catch (FormatException)
        {
            // Invalid Base64 format - not a PBKDF2 hash
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the stored password is using legacy plain text or simple hash format
    /// PBKDF2 hashes are always 48 bytes when decoded, resulting in 64 Base64 characters
    /// </summary>
    public bool IsLegacyFormat(string storedPassword)
    {
        if (string.IsNullOrEmpty(storedPassword))
            return true;

        try
        {
            byte[] decoded = Convert.FromBase64String(storedPassword);
            // PBKDF2 format is exactly SaltSize + HashSize = 48 bytes
            return decoded.Length != SaltSize + HashSize;
        }
        catch (FormatException)
        {
            // Not valid Base64 - definitely legacy (plain text)
            return true;
        }
    }

    /// <summary>
    /// Generates cryptographically secure random salt
    /// </summary>
    private static byte[] GenerateSalt()
    {
        byte[] salt = new byte[SaltSize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }

    /// <summary>
    /// Performs PBKDF2 key derivation with specified salt
    /// </summary>
    private static byte[] HashPasswordWithSalt(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256);

        return pbkdf2.GetBytes(HashSize);
    }
}
