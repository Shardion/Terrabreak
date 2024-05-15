using Microsoft.EntityFrameworkCore;

namespace Shardion.Terrabreak.Services.Database
{
    public class TerrabreakDatabaseContext : DbContext
    {
        public TerrabreakDatabaseContext(DbContextOptions<TerrabreakDatabaseContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TerrabreakDatabaseContext).Assembly);
        }
    }
}
