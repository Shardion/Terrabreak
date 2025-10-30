using System.Collections.Generic;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

public sealed record EnemyExtinctionBall : IEnemy
{
    public string Name => "Extinction Ball";
    public string Description => "A sentient orb that radiates a powerful aura of pain. Highly explosive.";
    public int Credits => 15222;
    public int PremultHealthMax => 175;
    public double TargetTotalPowerLevel => 6.00;

    public IReadOnlyList<EnemyAttack> Attacks(IReadOnlyDictionary<IPlayer, PlayerState> players) =>
    [
        new("Extinction", [], 90, 1),
        new("Not Nice", [], 35, 2),
        new("Not Nice", [], 35, 2),
    ];
}
