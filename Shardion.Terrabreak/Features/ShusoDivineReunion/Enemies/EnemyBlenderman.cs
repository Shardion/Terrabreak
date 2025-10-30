using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

public sealed record EnemyBlenderman : IEnemy
{
    public string Name => "Blenderman";
    public string Description => "An Anomaly-class entity with the capability to psychically terrorize.";
    public int Credits => 12522;
    public int PremultHealthMax => 175;
    public double TargetTotalPowerLevel => 5.00;

    public IReadOnlyList<EnemyAttack> Attacks(IReadOnlyDictionary<IPlayer, PlayerState> players)
    {
        List<EnemyAttack> attacks =
        [
            new("Blend", [], 77, 1),
            new("Black Crystal", [], 44, 2),
        ];

        if (players.Any(pair => !pair.Value.Debuffs.Contains(Debuff.Weakened)))
        {
            attacks.Add(new("Evoke", [Debuff.Weakened], 0, 4));
        }

        return attacks;
    }
}
