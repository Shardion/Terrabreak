using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

public sealed record EnemyTheSinger : IEnemy
{
    public string Name => "\"The Singer\"";
    public string InternalName => "TheSinger";
    public string Description => "Answers to all can be found above us.";
    public int Credits => 22;
    public int PremultHealthMax => 200;
    public double TargetTotalPowerLevel => 16.0;

    public IReadOnlyList<EnemyAttack> Attacks(IReadOnlyDictionary<IPlayer, PlayerState> players)
    {
        List<EnemyAttack> attacks =
        [
            new("Origin in Celestial Dust", [], 75, 4),
            new("Tragic Ending", [Debuff.Sealed], 150, 1),
            new("Binding Serenade", [Debuff.Sealed], 75, 2),
        ];

        if (players.Any(pair => !pair.Value.Debuffs.Contains(Debuff.Sealed)))
        {
            attacks.Add(new("Reunison", [Debuff.Sealed], 0, 4));
        }

        return attacks;
    }
}
