using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

public sealed record EnemyWeregamer : IEnemy
{
    public string Name => "Weregamer";
    public string Description => "On full moon nights, this terrible creature turns into an imitation of a god.";
    public int Credits => 5000;
    public int PremultHealthMax => 150;
    public double TargetTotalPowerLevel => 4.00;

    public IReadOnlyList<EnemyAttack> Attacks(IReadOnlyDictionary<IPlayer, PlayerState> players)
    {
        List<EnemyAttack> attacks =
        [
            new("Laser", [], 80, 1),
            new("Scratch", [], 40, 1),
        ];

        if (players.Any(pair => !pair.Value.Debuffs.Contains(Debuff.Weakened)))
        {
            attacks.Add(new("Matrix Wave", [Debuff.Weakened], 0, 2));
        }

        return attacks;
    }
}
