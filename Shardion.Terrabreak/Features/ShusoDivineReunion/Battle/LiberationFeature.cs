using System.Collections.Concurrent;
using System.Threading.Tasks;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;

public class LiberationFeature : ITerrabreakFeature
{
    public ConcurrentDictionary<ulong, bool> PlayersInBattles { get; } = [];

    public Task StartAsync()
    {
        return Task.CompletedTask;
    }
}
