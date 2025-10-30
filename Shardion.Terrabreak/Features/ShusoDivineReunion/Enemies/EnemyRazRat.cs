using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

public sealed record EnemyRazRat : IEnemy
{
    public string Name => "Raz Rat";
    public string Description => "A terrible machine that lords over all of the city rats.";
    public int Credits => 100;
    public int PremultHealthMax => 100;
    public double TargetTotalPowerLevel => 0.75;

    public IReadOnlyList<EnemyAttack> Attacks(IReadOnlyDictionary<IPlayer, PlayerState> players) =>
    [
        new("Rettack", [], 20, 1),
        new("Razzack", [], 30, 1),
    ];
}
