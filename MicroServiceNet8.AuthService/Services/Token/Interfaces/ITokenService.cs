using MicroServiceNet8.DTO.Auth;
using MicroServiceNet8.Entities.SYS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroServiceNet8.Auth.Services.Token.Interfaces
{
    public interface ITokenService
    {
        string GetClaimTypeId();
        string CreateToken(SYS_User user);
        RefreshToken GenerateRefreshToken();
    }
}
