using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Shardion.Terrabreak.Services.Database;

public class TerrabreakDesignTimeDbContextFactory : IDesignTimeDbContextFactory<TerrabreakDatabaseContext>
{
    public TerrabreakDatabaseContext CreateDbContext(string[] args)
    {
        ConfigurationManager manager = new();

        manager.Sources.Clear();
        manager.AddJsonFile(Path.Join(Entrypoint.GetConfigurationDirectoryPath(), "config.json"), true, false);

        return new TerrabreakDatabaseContext(
            new DbContextOptionsBuilder<TerrabreakDatabaseContext>()
                .UseSqlite(manager.GetRequiredSection("Database").GetValue<string>("ConnectionString"))
                .Options
        );
    }
}
