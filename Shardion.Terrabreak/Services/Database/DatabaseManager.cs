using System.Threading.Tasks;
using LiteDB;

namespace Shardion.Terrabreak.Services.Database
{
    public class DatabaseManager : ITerrabreakService
    {
        public LiteDatabase Database { get; }

        private readonly DatabaseManagerOptions _databaseOptions;

        public DatabaseManager(DatabaseManagerOptions databaseOptions)
        {
            _databaseOptions = databaseOptions;
            Database = new(_databaseOptions.ConnectionString);
        }

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }
    }
}
