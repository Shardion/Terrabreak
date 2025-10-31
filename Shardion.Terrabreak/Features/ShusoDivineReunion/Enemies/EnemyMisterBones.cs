using System.Collections.Generic;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

public sealed record EnemyMisterBones : IEnemy
{
    public string Name => "Mister Bones";
    public string Description => "A skeleton walking among us!";
    public int Credits => 250;
    public int PremultHealthMax => 150;
    public double TargetTotalPowerLevel => 1.5;

    public IReadOnlyList<EnemyAttack> Attacks(IReadOnlyDictionary<IPlayer, PlayerState> players) =>
    [
        new("Jumpscare", [Debuff.Sealed], 0, 1),
        new("Extremely Annoying Ranged Attack", [], 40, 1),
        new("Rattling Bones", [], 20, 2),
    ];
}
