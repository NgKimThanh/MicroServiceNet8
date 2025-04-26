using MicroServiceNet8.DTO.Auth;
using MicroServiceNet8.Entities.SYS;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthenNet8.Auth.Services.Token
{
    public class TokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Tạo Access Token, thời gian sống 1 ngày
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public string CreateToken(SYS_User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(_config["Jwt:NameId"], user.ID.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["AppSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Tạo Refresh Token, thời gian sống 7 ngày
        /// </summary>
        /// <returns></returns>
        public RefreshToken GenerateRefreshToken()
        {
            return new RefreshToken
            {
                Token = $"{Guid.NewGuid():N}{Guid.NewGuid():N}",
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(7), // 7 ngày
            };
        }
    }
}
