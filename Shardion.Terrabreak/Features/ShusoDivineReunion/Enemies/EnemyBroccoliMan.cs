using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

public sealed record EnemyBroccoliMan : IEnemy
{
    public string Name => "Broccoli Man";
    public string Description => "A head of broccoli, remarkable for the ability to use stilts.";
    public int Credits => 200;
    public int PremultHealthMax => 125;
    public double TargetTotalPowerLevel => 1.0;

    public IReadOnlyList<EnemyAttack> Attacks(IReadOnlyDictionary<IPlayer, PlayerState> players)
    {
        // Allowed to be a little stupid. If all players are burning, don't use Fantastic, but
        // don't avoid burning players when using it.
        List<EnemyAttack> attacks =
        [
            new("Dance", [], 25, 1),
            new("Dance", [], 25, 1),
        ];
        if (players.Any(pair => !pair.Value.Debuffs.Contains(Debuff.Burning)))
        {
            attacks.Add(new("Fantastic", [Debuff.Burning], 0, 1));
        }

        return attacks;
    }
}
