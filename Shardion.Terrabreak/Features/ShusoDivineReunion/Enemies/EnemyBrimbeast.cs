using System.Collections.Generic;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

public sealed record EnemyBrimbeast : IEnemy
{
    public string Name => "Brimbeast";
    public string Description => "A majestic, secondary quadruped, known for its incredible dashes, and the ability to fire lasers from its eyes.";
    public int Credits => 15222;
    public int PremultHealthMax => 175;
    public double TargetTotalPowerLevel => 5.50;

    public IReadOnlyList<EnemyAttack> Attacks(IReadOnlyDictionary<IPlayer, PlayerState> players) =>
    [
        new("Dash", [Debuff.Weakened], 35, 1),
        new("Laser Eyes", [], 50, 3),
    ];
}
