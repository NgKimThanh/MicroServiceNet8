using MicroServiceNet8.Services.Repositories.Users.Interfaces;
using MicroServiceNet8.Entities.SYS;
using MicroServiceNet8.Entity.DBContext;
using Microsoft.EntityFrameworkCore;

namespace AuthenNet8.Services.Repositories.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly DBContext _context;

        public UserRepository(DBContext context)
        {
            _context = context;
        }

        #region User
        public Task<SYS_User?> GetByEmailAsync(string email) =>
            _context.SYS_User.FirstOrDefaultAsync(u => u.Email == email);

        public Task<SYS_User?> GetByIdAsync(int id) =>
            _context.SYS_User.FirstOrDefaultAsync(u => u.ID == id);

        public Task<bool> EmailExistsAsync(string email) =>
            _context.SYS_User.AnyAsync(u => u.Email == email);

        public void AddUser(SYS_User user)
        {
            _context.SYS_User.Add(user);
        }
        #endregion User

        #region UserRefreshToken
        public void AddUserRefreshToken(SYS_UserRefreshToken userRefreshToken)
        {
            _context.SYS_UserRefreshToken.Add(userRefreshToken);
        }

        public Task<List<SYS_UserRefreshToken>> GetUserRefreshTokensAsync(int userId) =>
          _context.SYS_UserRefreshToken.Where(t => t.UserID == userId).ToListAsync();

        public Task<SYS_UserRefreshToken> GetUserRefreshTokenAsync(int userId, string token) =>
            _context.SYS_UserRefreshToken.FirstOrDefaultAsync(t => t.UserID == userId && t.Token == token);
        #endregion UserRefreshToken

        #region UserPasswordReset
        public void AddUserPasswordReset(SYS_UserPasswordReset userPasswordReset)
        {
            _context.SYS_UserPasswordReset.Add(userPasswordReset);
        }

        public Task<SYS_UserPasswordReset?> GetValidResetTokenAsync(string token) =>
            _context.SYS_UserPasswordReset.FirstOrDefaultAsync(u => u.ResetToken == token);

        public async Task RemoveUserPasswordReset(SYS_UserPasswordReset userPasswordReset)
        {
            _context.SYS_UserPasswordReset.Remove(userPasswordReset);
            await SaveChangesAsync();
        }
        #endregion UserPasswordReset

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}
