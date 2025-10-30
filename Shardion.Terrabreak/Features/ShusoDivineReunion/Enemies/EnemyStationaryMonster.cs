using System.Collections.Generic;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

public sealed record EnemyStationaryMonster : IEnemy
{
    public string Name => "Stationary Monster";
    public string Description => "A large creature with glaring eyes and extremely sharp teeth. Completely immobile, it appears to be a part of the landscape.";
    public int Credits => 550;
    public int PremultHealthMax => 125;
    public double TargetTotalPowerLevel => 2.50;

    public IReadOnlyList<EnemyAttack> Attacks(IReadOnlyDictionary<IPlayer, PlayerState> players) =>
    [
        new("False Wall", [Debuff.Sealed], 15, 1),
        new("Spike Teeth", [], 40, 1),
    ];
}
