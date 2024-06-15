using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Shardion.Terrabreak.Features.Bags
{
    public class BagsFeature : ITerrabreakFeature
    {
        public ConcurrentDictionary<string, PendingEntry> PendingEntries { get; } = [];

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }
    }
}
