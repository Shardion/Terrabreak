using System;
using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

public sealed record EnemyRetRat : IEnemy
{
    public string Name => "Ret Rat";
    public string Description => "A vicious rat that has learned to survive in the city.";
    public int Credits => 25;
    public int PremultHealthMax => 50;
    public double TargetTotalPowerLevel => 0.5;

    public IReadOnlyList<EnemyAttack> Attacks(IReadOnlyDictionary<IPlayer, PlayerState> players) => [
        new("Rettack", [], 15, 1),
    ];
}
