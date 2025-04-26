using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MicroServiceNet8.API.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;

        public JwtMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _config = config;
        }

        public async Task Invoke(HttpContext context)
        {
            // Lấy token từ cookie hoặc header
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (string.IsNullOrEmpty(token))
                token = context.Request.Cookies["accessToken"];

            if (!string.IsNullOrEmpty(token))
            {
                var principal = ValidateToken(token);
                if (principal != null)
                {
                    context.User = principal; // Gắn ClaimsPrincipal vào HttpContext
                }
            }

            await _next(context);
        }

        /// <summary>
        /// Lấy ClaimsPrincipal để gán cho HttpContext.User (HttpContext.User chứa các thông tin user như phân quyền,...)
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["AppSettings:SecretKey"]);

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true, // Kiểm tra iss claim trong token phải trùng với cấu hình (Jwt:Issuer)
                ValidateAudience = true, // Kiểm tra aud claim trong token phải đúng (Jwt:Audience)
                ValidateLifetime = true, //  Kiểm tra token có hết hạn chưa
                ValidateIssuerSigningKey = true, // Xác minh token có đúng chữ ký không

                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),

                ClockSkew = TimeSpan.Zero // Không cho phép chênh lệch thời gian (mặc định .NET cho phép lệch ±5 phút)
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, parameters, out SecurityToken validatedToken);

                // Kiểm tra lại Algorithm, Đảm bảo token được ký bằng thuật toán HmacSha512Signature
                if (validatedToken is JwtSecurityToken jwtToken &&
                   jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512Signature, StringComparison.InvariantCultureIgnoreCase))
                {
                    return principal;
                }
            }
            catch
            {
                // Có thể log lỗi nếu cần
            }

            return null;
        }
    }

}
