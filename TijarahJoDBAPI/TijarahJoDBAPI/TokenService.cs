using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace TijarahJoDBAPI
{
    public class TokenService(JwtOptions jwtOptions)
    {
        public string CreateAuthToken(int? UserId)
        {

            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor()
            {

                Issuer = jwtOptions.Issuer,
                Audience = jwtOptions.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
                , SecurityAlgorithms.HmacSha256),

                Expires = DateTime.UtcNow.AddMinutes(jwtOptions.Lifetime),
                Subject = new ClaimsIdentity(new Claim[]
                {
                  new Claim(ClaimTypes.NameIdentifier, UserId.ToString()),
                  //new (ClaimTypes.NameIdentifier,UserId.ToString()),
                })
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);

            var accessToken = tokenHandler.WriteToken(securityToken);

            return accessToken;
        }
    }
}
