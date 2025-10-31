using System;
using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

public sealed record EnemyParting : IEnemy
{
    public string Name => "\"Parting\"";
    public string InternalName => "Parting";
    public string Description => "Past and future, they weave together as \"present.\"";
    public int Credits => 22;
    public int PremultHealthMax => 225;
    public double TargetTotalPowerLevel => 16.00;

    public IReadOnlyList<EnemyAttack> Attacks(IReadOnlyDictionary<IPlayer, PlayerState> players)
    {
        List<EnemyAttack> attacks =
        [
            new("Crystal Hammer", [], 150, 1),
            new("Sight Beyond", [Debuff.Weakened], 95, 1),
            new("Redo", [], 0, 1, UntargetedDamage: 75, UntargetedDebuffs: []),
        ];

        if (players.Any(pair => !pair.Value.Debuffs.Contains(Debuff.Weakened)))
        {
            attacks.Add(new("Undo", [Debuff.Weakened], 75, 1, UntargetedDamage: 0, UntargetedDebuffs: [Debuff.Weakened]));
        }

        return attacks;
    }
}
