using System.Collections.Generic;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

public sealed record EnemyLineBreaker : IEnemy
{
    public string Name => "BGN-4 \"Line Breaker\"";
    public string InternalName => "LineBreaker";
    public string Description => "An autonomous attack vehicle, used during assaults on buildings and urban areas.";
    public int Credits => 850;
    public int PremultHealthMax => 150;
    public double TargetTotalPowerLevel => 5.5;

    public IReadOnlyList<EnemyAttack> Attacks(IReadOnlyDictionary<IPlayer, PlayerState> players) =>
    [
        new("Widespread Gunfire", [], 25, 4),
        new("Focused Gunfire", [], 50, 2),
        new("Sharp Trample", [], 75, 1),
    ];
}
