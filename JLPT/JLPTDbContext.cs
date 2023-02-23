using Catfish.API.Repository;
using Microsoft.EntityFrameworkCore;

namespace JLPT
{
    public class JLPTDbContext : RepoDbContext
    {
        public JLPTDbContext(DbContextOptions<RepoDbContext> options) : base(options)
        {
        }
    }
}
