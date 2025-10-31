using System.Collections.Generic;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

public sealed record EnemyPlagueMaster : IEnemy
{
    public string Name => "Plague Master";
    public string Description => "A freak accident with lightning allowed this creature to inflict vile diseases.";
    public int Credits => 500;
    public int PremultHealthMax => 150;
    public double TargetTotalPowerLevel => 3.50;

    public IReadOnlyList<EnemyAttack> Attacks(IReadOnlyDictionary<IPlayer, PlayerState> players) =>
    [
        new("Diseased Cloud", [], 30, 4),
        new("Hatred Claw", [Debuff.Burning, Debuff.Sealed], 60, 1),
    ];
}
