using Microsoft.EntityFrameworkCore;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

[Owned]
public sealed record EnemyRecord(string EnemyId, ulong SourceChannelId)
{
    public IEnemy Enemy => SdrRegistries.Enemies.Forward[EnemyId];
}
