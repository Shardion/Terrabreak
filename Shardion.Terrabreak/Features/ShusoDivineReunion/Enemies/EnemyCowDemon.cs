using System;
using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

public sealed record EnemyCowDemon : IEnemy
{
    public string Name => "Cow-demon";
    public string InternalName => "CowDemon";
    public string Description => "A low-level goon of cow-Lucifer, with only rudimentary tools to apprehend \"heretics.\"";
    public int Credits => 250;
    public int PremultHealthMax => 150;
    public double TargetTotalPowerLevel => 1.25;

    public IReadOnlyList<EnemyAttack> Attacks(IReadOnlyDictionary<IPlayer, PlayerState> players) =>
    [
        new("Revolver", [], 30, 1),
        new("Revolver", [], 30, 1),
        new("Faulty Revolver", [], 0, 1),
    ];
}
