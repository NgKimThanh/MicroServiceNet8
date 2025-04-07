using MicroServiceNet8.Entities.SYS;
using Microsoft.EntityFrameworkCore;

namespace MicroServiceNet8.Entity.DBContext
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {

        }

        public DbSet<SYS_User>? SYS_User { get; set; } = null!;
        public DbSet<SYS_UserRefreshToken>? SYS_UserRefreshToken { get; set; } = null!;
        public DbSet<SYS_UserPasswordReset>? SYS_UserPasswordReset { get; set; } = null!;
    }
}
