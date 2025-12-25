# ?? Password Security Implementation - TijarahJo

## Overview

TijarahJo implements a robust password security system using **PBKDF2-HMAC-SHA256** for all new users while maintaining backward compatibility with legacy plain text passwords.

---

## ?? Architecture

```
UsersController
      ?
ISecurityService ? SecurityService (Facade)
      ?
IPasswordHashingService ? PasswordHashingService (PBKDF2)
```

---

## 1?? Password Hashing (PBKDF2)

### Algorithm: PBKDF2-HMAC-SHA256

**File:** `TijarahJoDBAPI/Services/Security/PasswordHashingService.cs`

### Configuration Parameters

| Parameter | Value | Description |
|-----------|-------|-------------|
| **Salt Size** | 16 bytes (128 bits) | Unique random salt per password |
| **Hash Size** | 32 bytes (256 bits) | Output hash length |
| **Iterations** | 100,000 | OWASP recommended minimum |
| **Algorithm** | SHA256 | Cryptographic hash function |

### How It Works

#### Registration (Hashing)

```csharp
public string HashPassword(string password)
{
    // 1. Generate cryptographically secure random salt (16 bytes)
    byte[] salt = GenerateSalt();
    
    // 2. Hash password with PBKDF2 (100,000 iterations)
    byte[] hash = HashPasswordWithSalt(password, salt);
    
    // 3. Combine: [Salt (16 bytes)][Hash (32 bytes)] = 48 bytes
    byte[] combined = new byte[SaltSize + HashSize];
    Array.Copy(salt, 0, combined, 0, SaltSize);
    Array.Copy(hash, 0, combined, SaltSize, HashSize);
    
    // 4. Return Base64 for database storage
    return Convert.ToBase64String(combined);
}
```

**Storage Format:**
```
Base64([16-byte-salt][32-byte-hash]) = 64 characters
Example: "r5KJH3mP7Q8xN2vLkF9Y..."
```

#### Login (Verification)

```csharp
public bool VerifyPassword(string hashedPassword, string providedPassword)
{
    // 1. Decode Base64
    byte[] hashBytes = Convert.FromBase64String(hashedPassword);
    
    // 2. Extract salt (first 16 bytes)
    byte[] salt = hashBytes[0..SaltSize];
    
    // 3. Extract stored hash (last 32 bytes)
    byte[] storedHash = hashBytes[SaltSize..];
    
    // 4. Hash provided password with same salt
    byte[] providedHash = HashPasswordWithSalt(providedPassword, salt);
    
    // 5. Constant-time comparison (prevents timing attacks)
    return CryptographicOperations.FixedTimeEquals(storedHash, providedHash);
}
```

---

## 2?? Security Features

### ? Implemented

| Feature | Description |
|---------|-------------|
| **Random Salt** | Unique 16-byte salt per password prevents rainbow table attacks |
| **High Iterations** | 100,000 rounds makes brute-force computationally expensive |
| **Timing Attack Protection** | `CryptographicOperations.FixedTimeEquals()` |
| **Cryptographic RNG** | `RandomNumberGenerator.Create()` for unpredictable salts |
| **One-Way Hash** | Cannot be reversed/decrypted |
| **OWASP Compliant** | Follows OWASP Password Storage guidelines |
| **Automatic Migration** | Legacy passwords rehashed on successful login |

---

## 3?? Service Registration

**File:** `Program.cs`

```csharp
// Register Security Services
builder.Services.AddScoped<IPasswordHashingService, PasswordHashingService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
```

---

## 4?? Usage in Controllers

### Registration

```csharp
[HttpPost("register")]
public ActionResult<UserResponse> Register([FromBody] CreateUserRequest request)
{
    // Hash password before storing
    string hashedPassword = _securityService.HashPassword(request.Password);
    
    var user = new UserBL(new UserModel(
        null,
        request.Username,
        hashedPassword,  // Securely hashed
        request.Email,
        // ...
    ));
    
    user.Save();
    return CreatedAtRoute(...);
}
```

### Login (Hybrid Verification)

```csharp
[HttpPost("login")]
public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
{
    // Find user by username/email
    var user = UserBL.FindByLogin(request.Login);
    
    // Verify password (supports both PBKDF2 and legacy formats)
    if (!_securityService.VerifyPassword(user.HashedPassword, request.Password))
        return Unauthorized();
    
    // Auto-migrate legacy passwords to PBKDF2
    if (_securityService.NeedsRehash(user.HashedPassword))
    {
        user.HashedPassword = _securityService.HashPassword(request.Password);
        user.Save();
    }
    
    // Generate JWT and return
    return Ok(new LoginResponse { ... });
}
```

### Password Change

```csharp
[HttpPut("{id}/password")]
public ActionResult ChangePassword(int id, [FromBody] ChangePasswordRequest request)
{
    var user = UserBL.Find(id);
    
    // Verify current password
    if (!_securityService.VerifyPassword(user.HashedPassword, request.CurrentPassword))
        return BadRequest("Current password is incorrect");
    
    // Hash and save new password
    user.HashedPassword = _securityService.HashPassword(request.NewPassword);
    user.Save();
    
    return NoContent();
}
```

---

## 5?? Legacy Password Migration

The system implements **automatic migration** from legacy plain text passwords:

```
Login Attempt
     ?
Find User by Username/Email
     ?
Try PBKDF2 Verification
     ?
Success? ? Continue ?
     ? (Failed)
Try Legacy Plain Text Comparison
     ?
Success? ? Rehash with PBKDF2 ? Save ? Continue ?
     ? (Failed)
Return Unauthorized ?
```

### Detection Logic

```csharp
public bool IsLegacyFormat(string storedPassword)
{
    try
    {
        byte[] decoded = Convert.FromBase64String(storedPassword);
        // PBKDF2 format is exactly 48 bytes (16 salt + 32 hash)
        return decoded.Length != 48;
    }
    catch (FormatException)
    {
        // Not valid Base64 = plain text (legacy)
        return true;
    }
}
```

---

## 6?? API Endpoints

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/users/register` | POST | None | Register new user |
| `/api/users/login` | POST | None | Authenticate user |
| `/api/users/{id}/password` | PUT | Required | Change password |
| `/api/users/me` | GET | Required | Get current user |

### Register Request

```json
POST /api/users/register
{
  "username": "john_doe",
  "password": "SecurePass123!",
  "email": "john@example.com",
  "firstName": "John",
  "lastName": "Doe"
}
```

### Login Request

```json
POST /api/users/login
{
  "login": "john_doe",  // or email
  "password": "SecurePass123!"
}
```

### Login Response

```json
{
  "user": {
    "userID": 1,
    "username": "john_doe",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "roleID": 3
  },
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresAt": "2025-01-20T02:00:00Z",
  "role": "User"
}
```

### Change Password Request

```json
PUT /api/users/1/password
{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewSecurePass456!",
  "confirmPassword": "NewSecurePass456!"
}
```

---

## 7?? Database Requirements

Run `DiagramsAndDocs/Database-Setup-Complete.sql` which includes:

```sql
-- SP_GetUserByLogin: Finds user by username or email
CREATE OR ALTER PROCEDURE [dbo].[SP_GetUserByLogin]
    @Login NVARCHAR(510)
AS
BEGIN
    SELECT UserID, Username, HashedPassword, Email, ...
    FROM TbUsers
    WHERE (Username = @Login OR Email = @Login)
      AND IsDeleted = 0;
END;
```

---

## 8?? Security Comparison

| Feature | PBKDF2 (Current) | Plain Text (Legacy) |
|---------|------------------|---------------------|
| Reversible | ? No | ? Yes (visible) |
| Brute-Force Resistant | ? 100k iterations | ? Instant |
| Unique Per User | ? Random salt | ? Same format |
| Timing Attack Safe | ? Constant-time | ? Vulnerable |
| OWASP Compliant | ? Yes | ? No |
| Key Compromise Impact | ? Minimal | ? All exposed |

---

## 9?? Files Created/Modified

| File | Purpose |
|------|---------|
| `Services/Security/IPasswordHashingService.cs` | Interface for hashing |
| `Services/Security/PasswordHashingService.cs` | PBKDF2 implementation |
| `Services/Security/ISecurityService.cs` | Security facade interface |
| `Services/Security/SecurityService.cs` | Hybrid verification |
| `Controllers/UsersController.cs` | Updated for secure auth |
| `DTOs/Requests/ChangePasswordRequest.cs` | Password change DTO |
| `BLL/UserBL.cs` | Added `FindByLogin()` |
| `DAL/UserData.cs` | Added `GetUserByLogin()` |
| `Program.cs` | Service registration |
| `Database-Setup-Complete.sql` | `SP_GetUserByLogin` SP |

---

## ?? Best Practices

1. **Never log passwords** - Not even hashed ones
2. **Use HTTPS** - Encrypt data in transit
3. **Enforce password policy** - Min length, complexity
4. **Implement rate limiting** - Prevent brute force
5. **Add account lockout** - After failed attempts
6. **Use secure cookies** - HttpOnly, Secure flags
7. **Validate on server** - Never trust client-side only

---

## ?? References

- [OWASP Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)
- [NIST Digital Identity Guidelines](https://pages.nist.gov/800-63-3/sp800-63b.html)
- [RFC 2898 - PBKDF2 Specification](https://www.rfc-editor.org/rfc/rfc2898)

---

**Last Updated:** January 2025
