using AuthenNet8.Services.Token;
using MicroServiceNet8.DTO.Auth;
using MicroServiceNet8.Entities.Repositories.Users.Interfaces;
using MicroServiceNet8.Entities.SYS;
using MicroServiceNet8.Helper;
using MicroServiceNet8.Services.Services.Auth.Interfaces;
using MicroServiceNet8.Services.Services.Email.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;

namespace AuthenNet8.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailService _emailService;
        private readonly TokenService _tokenService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IUserRepository userRepo,
            IEmailService emailService,
            TokenService tokenService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userRepo = userRepo;
            _emailService = emailService;
            _tokenService = tokenService;
            _httpContextAccessor = httpContextAccessor;
        }

        private void SetRefreshToken(RefreshToken refreshToken, int userID)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = refreshToken.Expires
            };
            _httpContextAccessor.HttpContext!.Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
            _httpContextAccessor.HttpContext!.Response.Cookies.Append("userID", userID.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = refreshToken.Expires
            });
        }

        private RefreshToken GetRefreshToken()
        {
            string refreshToken = string.Empty;
            var userID = -1;
            try
            {
                refreshToken = _httpContextAccessor.HttpContext!.Request.Cookies["refreshToken"] ?? string.Empty;
                // Có thể lưu userID này ở local storage để tránh bị js đánh cắp token
                userID = Convert.ToInt32(_httpContextAccessor.HttpContext!.Request.Cookies["userId"]);
            }
            catch
            {
                throw HelperFault.BusinessFault("", "", "No Refresh Token found.");
            }

            return new RefreshToken
            {
                RefeshToken = refreshToken,
                UserID = userID,
            };
        }

        private async Task UpdateUserRefreshToken(SYS_User user, RefreshToken refreshToken)
        {
            // Lấy thông tin trình duyệt
            var deviceInfo = _httpContextAccessor.HttpContext!.Request.Headers["User-Agent"].ToString();

            // Xóa các refresh token khác của user
            var userRefreshTokens = await _userRepo.GetUserRefreshTokensAsync(user.ID);

            // refesh token của user trên thiết bị hiện tại
            var currentToken = userRefreshTokens.FirstOrDefault(c => c.DeviceInfo == deviceInfo);
            if (currentToken != null)
            {
                currentToken.ModifiedDate = DateTime.UtcNow;
                currentToken.ModifiedBy = user.Email;
                currentToken.Token = refreshToken.Token;
                currentToken.TokenExpires = refreshToken.Expires;
            }
            else
            {
                currentToken = new SYS_UserRefreshToken
                {
                    UserID = user.ID,
                    Token = refreshToken.Token,
                    DeviceInfo = deviceInfo,
                    TokenExpires = refreshToken.Expires,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = user.Email,
                };
                _userRepo.AddUserRefreshToken(currentToken);
            }

            // Trong 1 thời điểm chỉ có 1 refresh token của 1 user trên 1 thiết bị
            foreach (var token in userRefreshTokens.Where(c => c.DeviceInfo != deviceInfo))
            {
                token.Token = string.Empty;
                token.TokenExpires = null;
            }

            await _userRepo.SaveChangesAsync();
        }

        #region Đăng ký
        public async Task<RegisterRequest> Auth_Register(RegisterRequest request)
        {
            var result = new RegisterRequest();

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = await _userRepo.GetByEmailAsync(request.Email);
            if (user != null)
            {
                throw HelperFault.BusinessFault("", "", "Email already exists");
            }
            else
            {
                _userRepo.AddUser(new SYS_User
                {
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = request.Email,
                    PasswordHash = Convert.ToBase64String(passwordHash),
                    PasswordSalt = Convert.ToBase64String(passwordSalt)
                });
                await _userRepo.SaveChangesAsync();
            }

            result.Email = request.Email;
            return result;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        #endregion Đăng ký

        #region Đăng nhập
        public async Task<string> Auth_Login(LoginRequest rqUser)
        {
            var user = await _userRepo.GetByEmailAsync(rqUser.Email)
                ?? throw HelperFault.BusinessFault("", "", "User not found");

            var hash = Convert.FromBase64String(user.PasswordHash);
            var salt = Convert.FromBase64String(user.PasswordSalt);

            if (!VerifyPasswordHash(rqUser.Password, hash, salt))
                throw HelperFault.BusinessFault("", "", "Wrong password.");

            var jwt = _tokenService.CreateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            SetRefreshToken(refreshToken, user.ID);
            await UpdateUserRefreshToken(user, refreshToken);
            return jwt;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
        #endregion Đăng nhập

        #region Refresh token
        public async Task<string> Auth_RefreshToken()
        {
            // Cookie refresh token
            var cookie = GetRefreshToken();

            // Lấy user từ HpptOnly Cookie userID
            var user = await _userRepo.GetByIdAsync(cookie.UserID)
                ?? throw HelperFault.BusinessFault("", "", "Invalid user.");
            var userRefreshToken = await _userRepo.GetUserRefreshTokenAsync(cookie.UserID, cookie.RefeshToken);
            if (userRefreshToken == null)
                throw HelperFault.BusinessFault("", "", "Invalid Refresh Token.");

            if (userRefreshToken.TokenExpires < DateTime.UtcNow)
                throw HelperFault.BusinessFault("", "", "Refresh Token expired.");

            string newToken = _tokenService.CreateToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            SetRefreshToken(newRefreshToken, user.ID);

            userRefreshToken.Token = newRefreshToken.Token;
            userRefreshToken.TokenExpires = newRefreshToken.Expires;
            await _userRepo.SaveChangesAsync();

            return newToken;
        }
        #endregion Refresh token

        #region Đăng xuất
        public async Task Auth_Logout()
        {
            // Cookie refresh token
            var cookie = GetRefreshToken();

            if (string.IsNullOrEmpty(cookie.RefeshToken))
                throw HelperFault.BusinessFault("", "", "No Refresh Token found.");

            var userRefreshTokens = await _userRepo.GetUserRefreshTokensAsync(cookie.UserID);
            foreach (var userRefreshToken in userRefreshTokens)
            {
                userRefreshToken.Token = string.Empty;
                userRefreshToken.TokenExpires = null;
            }
            await _userRepo.SaveChangesAsync();

            // Xóa cookie refresh token
            _httpContextAccessor.HttpContext!.Response.Cookies.Delete("refreshToken");
            _httpContextAccessor.HttpContext!.Response.Cookies.Delete("userID");
        }
        #endregion Đăng xuất

        #region Quên mật khẩu
        public async Task Auth_ForgotPassword(string email)
        {
            var user = await _userRepo.GetByEmailAsync(email)
                ?? throw HelperFault.BusinessFault("", "", "User not found.");

            var resetToken = Guid.NewGuid().ToString("N");
            var resetRecord = new SYS_UserPasswordReset
            {
                UserID = user.ID,
                ResetToken = resetToken,
                TokenExpires = DateTime.UtcNow.AddMinutes(30),
                CreatedDate = DateTime.UtcNow
            };

            _userRepo.AddUserPasswordReset(resetRecord);
            await _userRepo.SaveChangesAsync();

            var resetLink = $"https://yourapp.com/reset-password?token={resetToken}";
            await _emailService.Send(email, "Reset your password", $"Click the link to reset: {resetLink}");
        }

        public async Task Auth_ResetPassword(string token, string newPassword)
        {
            var resetRecord = await _userRepo.GetValidResetTokenAsync(token)
                ?? throw HelperFault.BusinessFault("", "", "Invalid token");
            if (resetRecord.TokenExpires < DateTime.UtcNow)
                throw HelperFault.BusinessFault("", "", "Token expired");

            var user = await _userRepo.GetByIdAsync(resetRecord.UserID)
                ?? throw HelperFault.BusinessFault("", "", "User not found");

            CreatePasswordHash(newPassword, out byte[] hash, out byte[] salt);
            user.PasswordHash = Convert.ToBase64String(hash);
            user.PasswordSalt = Convert.ToBase64String(salt);

            await _userRepo.RemoveUserPasswordReset(resetRecord);
            await _userRepo.SaveChangesAsync();
        }
        #endregion Quên mật khẩu
    }
}
