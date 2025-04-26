using MicroServiceNet8.Auth.Services.Auth.Interfaces;
using MicroServiceNet8.DTO.Auth;
using MicroServiceNet8.Services.Services.User;
using MicroServiceNet8.Services.Services.User.Interfaces;
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
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        #region Đăng ký
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Auth_Register([FromBody] RegisterRequest request)
        {
            return Ok(await _authService.Auth_Register(request));
        }
        #endregion Đăng ký

        #region Đăng nhập
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Auth_Login([FromBody] LoginRequest request)
        {
            return Ok(await _authService.Auth_Login(request));
        }
        #endregion Đăng nhập

        #region Refresh token
        [HttpPost("refresh-token")]
        [AllowAnonymous]
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
        [AllowAnonymous]
        public async Task<IActionResult> Auth_ForgotPassword([FromBody] PasswordResetRequest request)
        {
            // Gửi link khôi phục pass đến mail user
            await _authService.Auth_ForgotPassword(request.Email);
            return Ok();
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> Auth_ResetPassword([FromBody] PasswordResetRequest request)
        {
            await _authService.Auth_ResetPassword(request.Token, request.NewPassword);
            return Ok();
        }
        #endregion Quên mật khẩu

        [HttpGet("get-current-user")]
        public IActionResult Auth_GetCurrentUser()
        {
            var user = _userService.GetCurrentUserFromHttpContextUser();
            return Ok(new
            {
                message = "Ok",
                id = user.ID,
                email = user.Email,
                fullName = $"{user.FirstName} {user.LastName}"
            });
        }
    }
}
