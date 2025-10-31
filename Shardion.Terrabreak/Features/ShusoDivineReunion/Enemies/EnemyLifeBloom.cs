using System;
using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

public sealed record EnemyLifeBloom : IEnemy
{
    public string Name => "Life Bloom";
    public string Description => "An extremely large jungle plant with surprisingly adept swordplay.";
    public int Credits => 4500;
    public int PremultHealthMax => 250;
    public double TargetTotalPowerLevel => 12;

    public IReadOnlyList<EnemyAttack> Attacks(IReadOnlyDictionary<IPlayer, PlayerState> players) =>
    [
        new("Devour", [], 60, 1),
        new("Vines", [], 40, 2),
        new("Sword Fight", [], 90, 1, UntargetedDamage: 20, UntargetedDebuffs:new List<Debuff>(0)),
    ];
}
