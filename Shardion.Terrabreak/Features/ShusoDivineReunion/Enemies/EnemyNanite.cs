using System.Collections.Generic;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

public sealed record EnemyNanite : IEnemy
{
    public string Name => "BGN-7 \"Nanite\"";
    public string InternalName => "Nanite";
    public string Description => "An autonomous repair platform. Can repair damaged machines in minutes, but only has maintenance tools for self-defense.";
    public int Credits => 300;
    public int PremultHealthMax => 125;
    public double TargetTotalPowerLevel => 2.0;

    public IReadOnlyList<EnemyAttack> Attacks(IReadOnlyDictionary<IPlayer, PlayerState> players) =>
    [
        new("Wrench", [], 15, 1),
        new("Screwdriver", [], 35, 1),
        new("Nail Gun", [], 35, 2),
    ];
}
