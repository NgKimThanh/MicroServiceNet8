using MicroServiceNet8.Auth.Services.User.Interfaces;
using MicroServiceNet8.Entities.SYS;
using MicroServiceNet8.Services.Services.User.Interfaces;

namespace MicroServiceNet8.Services.Services.User
{
    public class UserService : IUserService
    {
        private readonly ICurrentUser _currentUser;

        public UserService(ICurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public SYS_User GetCurrentUserFromHttpContextUser()
        {
            return new SYS_User
            {
                ID = _currentUser.UserID,
                Email = _currentUser.Email ?? string.Empty,
            };
        }
    }
}
