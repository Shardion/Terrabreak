using Shardion.Terrabreak.Services.Database;

namespace Shardion.Terrabreak.Services.Timeout
{
    public class TimeoutCollectionManager : AbstractCollectionManager<Timeout>
    {
        protected override string CollectionName => "timeout";

        public TimeoutCollectionManager(DatabaseManager database) : base(database)
        {
        }
    }
}
