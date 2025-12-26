using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TijarahJoDBAPI
{
    /// <summary>
    /// Service for creating and managing JWT tokens with role-based claims
    /// </summary>
    public class TokenService
    {
        private readonly JwtOptions _jwtOptions;

        public TokenService(JwtOptions jwtOptions)
        {
            _jwtOptions = jwtOptions;
        }

        /// <summary>
        /// Creates a JWT authentication token with user ID and role claims
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <param name="role">The user's role (e.g., "Admin", "User")</param>
        /// <param name="username">Optional username for additional claims</param>
        /// <returns>JWT token string</returns>
        public string CreateAuthToken(int? userId, string role, string? username = null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId?.ToString() ?? ""),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique token ID
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Add username if provided
            if (!string.IsNullOrEmpty(username))
            {
                claims.Add(new Claim(ClaimTypes.Name, username));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey)),
                    SecurityAlgorithms.HmacSha256),
                Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.Lifetime),
                Subject = new ClaimsIdentity(claims)
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(securityToken);
        }

        /// <summary>
        /// Legacy method for backward compatibility - defaults to "User" role
        /// </summary>
        [Obsolete("Use CreateAuthToken(userId, role) instead")]
        public string CreateAuthToken(int? userId)
        {
            return CreateAuthToken(userId, "User");
        }
    }
}
