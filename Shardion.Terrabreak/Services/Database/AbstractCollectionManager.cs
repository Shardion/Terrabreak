using System.Threading.Tasks;
using LiteDB;

namespace Shardion.Terrabreak.Services.Database
{
    public abstract class AbstractCollectionManager<TCollection> : ITerrabreakService
    {
        protected abstract string CollectionName { get; }
        protected DatabaseManager Database { get; }

        public ILiteCollection<TCollection> Collection { get; }

        public AbstractCollectionManager(DatabaseManager database)
        {
            Database = database;
            Collection = database.Database.GetCollection<TCollection>();
        }

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }
    }
}
