using MicroServiceNet8.Auth.Services.User.Interfaces;
using MicroServiceNet8.Entities.SYS;

namespace MicroServiceNet8.Services.Services.User
{
    public class UserService
    {
        private readonly ICurrentUser _currentUser;

        public UserService(ICurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public async Task<SYS_User> GetCurrentUser()
        {
            return new SYS_User
            {
                ID = _currentUser.UserID,
                Email = _currentUser.Email ?? string.Empty,
            };
        }
    }
}
