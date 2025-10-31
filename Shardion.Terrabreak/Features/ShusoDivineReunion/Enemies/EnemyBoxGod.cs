using System;
using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

public sealed record EnemyBoxGod : IEnemy
{
    public string Name => "BOX GOD";
    public string Description => "You are not ready.";
    public int Credits => 41000000;
    public int PremultHealthMax => 250;
    public double TargetTotalPowerLevel => 16.00;

    public IReadOnlyDictionary<IPlayer, EnemyHit> Attack(EnemyState state, IReadOnlyDictionary<IPlayer, PlayerState> players)
    {
        Dictionary<IPlayer, EnemyHit> hits = [];
        if (state.ScratchCounter <= 0 && state.Health <= PremultHealthMax * TargetTotalPowerLevel / 2)
        {
            state.ScratchCounter = 1;
            foreach ((IPlayer player, PlayerState playerState) in players)
            {
                hits.Add(player, new("Realm of Steel and Stone", [Debuff.Subjugation], 150));
            }

            return hits;
        }

        IReadOnlyList<EnemyAttack> attacks = Attacks(players);
        EnemyAttack randomAttack = attacks[Random.Shared.Next(attacks.Count)];
        IEnumerable<KeyValuePair<IPlayer, PlayerState>> randomPlayers = players.Shuffle().Take(randomAttack.Targets);
        if (randomAttack.AttackName == "The Mechanism")
        {
            foreach ((IPlayer player, PlayerState playerState) in randomPlayers)
            {
                Debuff randomDebuff = randomAttack.Debuffs.Shuffle().First();
                hits.Add(player, new(randomAttack.AttackName, [randomDebuff], 0));
            }

            return hits;
        }

        int currentPlayerCount = 0;
        foreach ((IPlayer player, PlayerState playerState) in randomPlayers)
        {
            if (currentPlayerCount >= randomAttack.Targets)
            {
                if (randomAttack.UntargetedDamage is not int untargetedDamage ||
                    randomAttack.UntargetedDebuffs is not IReadOnlyCollection<Debuff> untargetedDebuffs)
                {
                    break;
                }
                hits.Add(player, new(randomAttack.AttackName, untargetedDebuffs, untargetedDamage));
            }
            else
            {
                hits.Add(player, new(randomAttack.AttackName, randomAttack.Debuffs, randomAttack.Damage));
                currentPlayerCount++;
            }
        }

        return hits;
    }

    public IReadOnlyList<EnemyAttack> Attacks(IReadOnlyDictionary<IPlayer, PlayerState> players)
    {
        List<EnemyAttack> attacks = [
            new("Boxing Match", [], 125, 1),
            new("Unrelenting Assault", [], 75, 2),
        ];

        if (players.All(player => player.Value.Debuffs.Count <= 0))
        {
            attacks.Add(new("The Symbol of Destruction", [Debuff.Burning], 41, 2));
            attacks.Add(new("The Mechanism", [Debuff.Burning, Debuff.Weakened, Debuff.Subjugation, Debuff.Sealed], 0, 2));
        }

        return attacks;
    }
}
