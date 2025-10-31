using System;
using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

public sealed record EnemyFormless : IEnemy
{
    public string Name => "Formless";
    public string Description => "Hahahahaha...";
    public int Credits => 6000;
    public int PremultHealthMax => 250;
    public double TargetTotalPowerLevel => 14.00;

    public IReadOnlyDictionary<IPlayer, EnemyHit> Attack(EnemyState state, IReadOnlyDictionary<IPlayer, PlayerState> players)
    {
        Dictionary<IPlayer, EnemyHit> hits = [];
        bool useScreech = Random.Shared.Next(2) == 1;
        KeyValuePair<IPlayer, PlayerState> randomPlayer = players.Shuffle().First();
        if (useScreech)
        {
            Debuff[] possibleDebuffs = [Debuff.Weakened, Debuff.Burning, Debuff.Sealed];
            Debuff randomDebuff = possibleDebuffs[Random.Shared.Next(possibleDebuffs.Length)];
            hits.Add(randomPlayer.Key, new("Horrific Screech", [randomDebuff], 0));
        }
        else
        {
            hits.Add(randomPlayer.Key, new($"{randomPlayer.Key.Weapon.Name}", [], randomPlayer.Key.Weapon.DealDamage(randomPlayer.Key, this) * 3));
        }
        return hits;
    }

    public IReadOnlyList<EnemyAttack> Attacks(IReadOnlyDictionary<IPlayer, PlayerState> players)
    {
        return new List<EnemyAttack>([
            new EnemyAttack("WTF", [Debuff.Weakened, Debuff.Burning, Debuff.Subjugation, Debuff.Sealed], 100000, 4)
        ]);
    }
}
