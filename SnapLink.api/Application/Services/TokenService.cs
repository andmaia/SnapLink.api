using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SnapLink.api.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly string _jwtKey;

        public TokenService()
        {
            _jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
                ?? throw new InvalidOperationException("JWT_KEY não encontrada nas variáveis de ambiente.");
        }

        public string GeneratePageToken(string pageId)
        {
            var claims = new[]
            {
                new Claim("pageId", pageId)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30), 
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
