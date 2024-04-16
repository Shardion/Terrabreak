using LiteDB;

namespace Shardion.Terrabreak.Services.Database
{
    public class DatabaseManager
    {
        public LiteDatabase Database { get; }

        private readonly DatabaseManagerOptions _databaseOptions;

        public DatabaseManager(DatabaseManagerOptions databaseOptions)
        {
            _databaseOptions = databaseOptions;
            Database = new(_databaseOptions.ConnectionString);
        }
    }
}
