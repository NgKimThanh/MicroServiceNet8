using MicroServiceNet8.DTO.Auth;
using MicroServiceNet8.Services.Services.Auth.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MicroServiceNet8.Services.API.Controllers.Auth
{
    [Route("api/auth/[controller]")]
    [Authorize]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        #region Đăng ký
        [HttpPost("register")]
        public async Task<IActionResult> Auth_Register([FromBody] RegisterRequest request)
        {
            return Ok(await _authService.Auth_Register(request));
        }
        #endregion Đăng ký

        #region Đăng nhập
        [HttpPost("login")]
        public async Task<IActionResult> Auth_Login([FromBody] LoginRequest request)
        {
            return Ok(await _authService.Auth_Login(request));
        }
        #endregion Đăng nhập

        #region Refresh token
        [HttpPost("refresh-token")]
        public async Task<IActionResult> Auth_RefreshToken()
        {
            return Ok(await _authService.Auth_RefreshToken());
        }
        #endregion Refresh token

        #region Đăng xuất
        [HttpPost("logout")]
        public async Task<IActionResult> Auth_Logout()
        {
            await _authService.Auth_Logout();
            return Ok();
        }
        #endregion Đăng xuất

        #region Quên mật khẩu
        [HttpPost("forgot-password")]
        public async Task<IActionResult> Auth_ForgotPassword([FromBody] PasswordResetRequest request)
        {
            // Gửi link khôi phục pass đến mail user
            await _authService.Auth_ForgotPassword(request.Email);
            return Ok();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> Auth_ResetPassword([FromBody] PasswordResetRequest request)
        {
            await _authService.Auth_ResetPassword(request.Token, request.NewPassword);
            return Ok();
        }
        #endregion Quên mật khẩu
    }
}
