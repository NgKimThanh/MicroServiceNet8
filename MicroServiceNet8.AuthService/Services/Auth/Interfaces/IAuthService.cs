using MicroServiceNet8.DTO.Auth;

namespace MicroServiceNet8.Auth.Services.Auth.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterRequest> Auth_Register(RegisterRequest request);

        Task<string> Auth_Login(LoginRequest rqUser);

        Task<string> Auth_RefreshToken();

        Task Auth_Logout();

        Task Auth_ForgotPassword(string email);

        Task Auth_ResetPassword(string token, string newPassword);
    }
}
