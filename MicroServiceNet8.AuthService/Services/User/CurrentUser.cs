using MicroServiceNet8.Auth.Services.User.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MicroServiceNet8.Auth.Services.User
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _context;

        public CurrentUser(IHttpContextAccessor context)
        {
            _context = context;
        }

        private ClaimsPrincipal? User => _context.HttpContext?.User;

        public int UserID => int.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? 
            id : 
            -1;
        public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value;
        public string? Name => User?.Identity?.Name;
        //public IEnumerable<string> Roles => User?.FindAll(ClaimTypes.Role)?.Select(r => r.Value) ?? Enumerable.Empty<string>();
    }
}
