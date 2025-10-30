using System;
using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

public sealed record EnemyTwinespinner : IEnemy
{
    public string Name => "\"Twinespinner\"";
    public string InternalName => "Twinespinner";
    public string Description => "Past and future, they weave together as \"present.\"";
    public int Credits => 22;
    public int PremultHealthMax => 200;
    public double TargetTotalPowerLevel => 8.00;

    public IReadOnlyList<EnemyAttack> Attacks(IReadOnlyDictionary<IPlayer, PlayerState> players)
    {
        List<EnemyAttack> attacks =
        [
            new("Fool's Errand", [], 150, 1),
            new("Solar Interleaving", [], 75, 1, UntargetedDamage: 0, UntargetedDebuffs: [Debuff.Burning]),
        ];

        if (players.Any(pair => !pair.Value.Debuffs.Contains(Debuff.Burning)))
        {
            attacks.Add(new("Demon Flame", [Debuff.Burning], 0, Random.Shared.Next(4) + 1));
        }

        return attacks;
    }
}
