using MicroServiceNet8.Entities.SYS;
using MicroServiceNet8.Repositories.Users.Interfaces;
using System.Security.Claims;

namespace MicroServiceNet8.Auth.Services.Google
{
    public class GoogleLoginService
    {
        private readonly IUserRepository _userRepo;

        public GoogleLoginService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<SYS_User> HandleGoogleUserLoginAsync(ClaimsPrincipal userClaims)
        {
            // Ensure the required namespace is included for FindFirstValue
            var email = userClaims?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = userClaims?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            var existingUser = await _userRepo.GetByEmailAsync(email);
            if (existingUser != null)
                return existingUser;

            var newUser = new SYS_User
            {
                Email = email,
                FirstName = name ?? "Google User",
                LastName = "",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "GoogleOAuth"
            };

            _userRepo.AddUser(newUser);
            await _userRepo.SaveChangesAsync();

            return newUser;
        }
    }
}