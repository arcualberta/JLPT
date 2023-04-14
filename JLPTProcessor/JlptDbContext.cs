using Microsoft.EntityFrameworkCore;

namespace JLPT
{
    public class JlptDbContext : DbContext
    {
        public JlptDbContext(DbContextOptions<JlptDbContext> options)
           : base(options)
        {
        }
        public DbSet<UserInfo>? UsersInfo { get; set; }
    }
}
