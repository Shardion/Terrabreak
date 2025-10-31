using System.Collections.Generic;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

public sealed record EnemySilkseeker : IEnemy
{
    public string Name => "\"Silkseeker\"";
    public string InternalName => "Silkseeker";
    public string Description => "Now without purpose, this terrifying security robot fell into disrepair, but is still capable of causing extreme damage.";
    public int Credits => 3000;
    public int PremultHealthMax => 150;
    public double TargetTotalPowerLevel => 10;

    public IReadOnlyList<EnemyAttack> Attacks(IReadOnlyDictionary<IPlayer, PlayerState> players) =>
    [
        new("Neurolaser", [], 150, 2),
        new("Nope!", [], 0, 0),
        new("Nail Crash", [], 150, 1),
    ];
}
