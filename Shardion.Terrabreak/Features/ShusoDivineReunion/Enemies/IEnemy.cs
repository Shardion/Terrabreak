using System;
using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;

public interface IEnemy : INamedEntity
{
    public int HealthMax => Convert.ToInt32(PremultHealthMax * TargetTotalPowerLevel);

    public int Credits { get; }
    public int PremultHealthMax { get; }
    public double TargetTotalPowerLevel { get; }

    public IReadOnlyDictionary<IPlayer, EnemyHit> Attack(EnemyState state, IReadOnlyDictionary<IPlayer, PlayerState> players)
    {
        IReadOnlyList<EnemyAttack> attacks = Attacks(players);
        EnemyAttack randomAttack = attacks[Random.Shared.Next(attacks.Count)];

        IEnumerable<KeyValuePair<IPlayer, PlayerState>> randomPlayers = players.Shuffle();
        Dictionary<IPlayer, EnemyHit> hits = [];
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
                hits.Add(player, new EnemyHit(randomAttack.AttackName, untargetedDebuffs, untargetedDamage));
            }
            else
            {
                hits.Add(player, new EnemyHit(randomAttack.AttackName, randomAttack.Debuffs, randomAttack.Damage));
                currentPlayerCount++;
            }
        }

        return hits;
    }

    protected IReadOnlyList<EnemyAttack> Attacks(IReadOnlyDictionary<IPlayer, PlayerState> players);
}
