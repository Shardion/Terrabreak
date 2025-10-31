using System.Collections.Generic;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

public sealed record EnemyLegendaryWarrior : IEnemy
{
    public string Name => "\"The Legendary Warrior\"";
    public string InternalName => "LegendaryWarrior";
    public string Description => "An incredibly-fierce, incredibly-large insect, with a dangerous nail, and a long history.";
    public int Credits => 400;
    public int PremultHealthMax => 125;
    public double TargetTotalPowerLevel => 3.00;

    public IReadOnlyList<EnemyAttack> Attacks(IReadOnlyDictionary<IPlayer, PlayerState> players) =>
    [
        new("Shockwaves", [], 15, 2),
        new("Shockwaves", [], 15, 2),
        new("Shockwaves", [], 15, 2),
        new("Life Enders", [], 40, 1),
        new("Life Enders", [], 40, 1),
        new("Life Enders", [], 40, 1),
        new("\"Lethal One-Hit-Kill Nail\"", [], 80, 1),
    ];
}
