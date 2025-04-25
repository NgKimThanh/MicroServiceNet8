using MicroServiceNet8.Auth.Services.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MicroServiceNet8.API.Controllers.User
{
    [Authorize]
    [ApiController]
    [Route("api/user/[controller]")]
    public class GoogleController : ControllerBase
    {

        private readonly GoogleLoginService _googleLoginService;

        public GoogleController(GoogleLoginService googleLoginService)
        {
            _googleLoginService = googleLoginService;
        }

        [HttpGet("get-user-info")]
        public async Task<IActionResult> GetUserInfo()
        {
            var user = await _googleLoginService.HandleGoogleUserLoginAsync(User);

            return Ok(new
            {
                message = "Đã xác thực và lưu user nếu lần đầu login",
                email = user.Email,
                fullName = $"{user.FirstName} {user.LastName}"
            });
        }
    }
}
