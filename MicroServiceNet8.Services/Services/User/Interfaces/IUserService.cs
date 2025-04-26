using MicroServiceNet8.Entities.SYS;

namespace MicroServiceNet8.Services.Services.User.Interfaces
{
    public interface IUserService
    {
        SYS_User GetCurrentUserFromHttpContextUser();
    }
}
