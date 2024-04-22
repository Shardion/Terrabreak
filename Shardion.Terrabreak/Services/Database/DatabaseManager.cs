using System;
using System.Threading.Tasks;
using LiteDB;

namespace Shardion.Terrabreak.Services.Database
{
    public class DatabaseManager : ITerrabreakService, IDisposable
    {
        public LiteDatabase Database { get; }

        private readonly DatabaseManagerOptions _databaseOptions;
        private bool _disposed;

        public DatabaseManager(DatabaseManagerOptions databaseOptions)
        {
            _databaseOptions = databaseOptions;
            Database = new(_databaseOptions.ConnectionString);
        }

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(disposingManagedObjects: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposingManagedObjects)
        {
            if (_disposed)
            {
                return;
            }

            if (disposingManagedObjects)
            {
                Database.Dispose();
            }

            _disposed = true;
        }
    }
}
