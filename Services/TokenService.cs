using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MecHub.Services
{
    public class TokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GerarToken(string email, string nome, string perfil, int mecanicoId)
        {
            var key = _config["Jwt:Key"]
                ?? "SuaChaveSuperSecretaEMuitoLongaParaODevelopmentMecHub2026!";
            var issuer = _config["Jwt:Issuer"] ?? "MecHubAPI";
            var audience = _config["Jwt:Audience"] ?? "MecHubApp";

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Name, nome),
                new Claim(ClaimTypes.Role, perfil),
                new Claim("MecanicoId", mecanicoId.ToString()),   // 🚨 NOVA CLAIM
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var keyBytes = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(keyBytes, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}