using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;

public class BattleEngine
{
    public IEnemy Enemy { get; }
    public EnemyState EnemyState { get; }
    public IReadOnlyDictionary<IPlayer, PlayerState> Players { get; }
    public Queue<string> BattleLog { get; } = new(22);
    public bool NextTurnIsEnemy { get; set; }

    public BattleEngine(IEnemy enemy, IReadOnlyCollection<IPlayer> players)
    {
        ConcurrentDictionary<IPlayer, PlayerState> playersWithStates = [];
        foreach (IPlayer player in players)
        {
            playersWithStates[player] = new()
            {
                Health = player.HealthMax,
            };
        }
        Players = playersWithStates;

        Enemy = enemy;
        EnemyState = new EnemyState
        {
            Health = enemy.HealthMax,
        };
    }

    private void LogLine(string line)
    {
        if (BattleLog.Count >= 22)
        {
            _ = BattleLog.Dequeue();
        }
        BattleLog.Enqueue(line);
    }

    public void Start()
    {
        LogLine($"**{Enemy.Name}** appears!");
        LogLine($"- {Enemy.Description}");
    }

    public BattleResult? Turn()
    {
        Victor? maybeVictor;
        if (NextTurnIsEnemy)
        {
            NextTurnIsEnemy = false;
            maybeVictor = TurnEnemy();
        }
        else
        {
            NextTurnIsEnemy = true;
            maybeVictor = TurnPlayer();
        }

        if (maybeVictor is Victor victor)
        {
            if (victor is Victor.Enemies)
            {
                LogLine($"{(Players.Count > 1 ? "Everyone has" : "You have")} fallen. The battle is lost.");
                return new BattleResult(victor, 0);
            }
            if (victor is Victor.Players)
            {
                int credits = VariateCredits(Enemy.Credits) / Players.Count;
                LogLine($"**The battle has been won!** {(Players.Count > 1 ? "Everyone" : "You")} earned <:credit:1426414005957689445> **{credits}**!");
                return new BattleResult(victor, credits);
            }
        }
        else
        {
            foreach ((IPlayer player, PlayerState playerState) in Players)
            {
                if (player is not HelperPlayer)
                {
                    continue;
                }
                // If the player has Burning, Weakened, or Subjugation, cure immediately
                bool burning = playerState.Debuffs.Contains(Debuff.Burning);
                bool weakened = playerState.Debuffs.Contains(Debuff.Weakened);
                bool subjugation = playerState.Debuffs.Contains(Debuff.Subjugation);
                if (burning || weakened || subjugation)
                {
                    if (playerState.CureUses < player.Cure?.GetMaxUses(player))
                    {
                        playerState.QueuedIntervention = Intervention.UseCure;
                    }
                }

                // Heal, but if the player has Sealed, cure that first
                int playerHealth = Convert.ToInt32(Math.Round((decimal)playerState.Health / player.HealthMax * 100));
                if (playerHealth <= 50 && playerState.HealUses < 1)
                {
                    if (playerState.Debuffs.Contains(Debuff.Sealed))
                    {
                        if (playerState.CureUses < player.Cure?.GetMaxUses(player))
                        {
                            playerState.QueuedIntervention = Intervention.UseCure;
                        }
                    }
                    else
                    {
                        playerState.QueuedIntervention = Intervention.UseHeal;
                    }
                }
            }
        }

        return null;
    }

    public Victor? TurnPlayer()
    {
        foreach ((IPlayer player, PlayerState playerState) in Players)
        {
            if (playerState.Health <= 0)
            {
                continue;
            }

            if (playerState.QueuedIntervention is Intervention intervention)
            {
                if (intervention == Intervention.UseCure)
                {
                    bool playerHasDebuffs = playerState.Debuffs.Count > 0;
                    bool playerHasCureUses = playerState.CureUses < player.Cure?.GetMaxUses(player);
                    if (player.Cure is not null && playerHasDebuffs && playerHasCureUses)
                    {
                        string debuffsText = "debuffs";
                        if (playerState.Debuffs.Count == 1)
                        {
                            debuffsText = playerState.Debuffs.First().ToString();
                        }

                        int remainingCureUses = player.Cure.GetMaxUses(player) - playerState.CureUses - 1;
                        LogLine($"**{player.Name}** uses {player.Cure.Name} to cure {debuffsText}! **{remainingCureUses}** use{(remainingCureUses == 1 ? "" : "s")} remain.");
                        playerState.Debuffs.Clear();
                        playerState.CureUses++;
                    }
                }
                else if (intervention == Intervention.UseHeal)
                {
                    if (!playerState.Debuffs.Contains(Debuff.Sealed))
                    {
                        bool playerHasDamage = playerState.Health < player.HealthMax;
                        bool playerHasHealUses = playerState.HealUses < 1;
                        if (player.Heal is not null && playerHasDamage && playerHasHealUses)
                        {
                            LogLine($"**{player.Name}** consumes {player.Heal.Name} and heals!");
                            playerState.Health = int.Min(playerState.Health + player.Heal.Heal(player),
                                player.HealthMax);
                            playerState.HealUses++;
                        }
                    }
                }
                else if (intervention == Intervention.UseRibbon)
                {
                    if (!playerState.Debuffs.Contains(Debuff.Sealed))
                    {
                        if (playerState.RibbonUses < player.Ribbons)
                        {
                            LogLine($"**{player.Name}** uses a ribbon!");
                            LogLine(
                                $"- {SdrRegistries.RibbonResponses[Random.Shared.Next(SdrRegistries.RibbonResponses.Count)]}");
                            LogLine($"**{Enemy.Name}** was profoundly insulted!");
                            int damage = Random.Shared.Next(10);
                            EnemyState.Health -= VariateDamage(damage);
                            playerState.RibbonUses++;
                        }
                    }
                }
                playerState.QueuedIntervention = null;
            }

            if (PlayerAttacks(player, playerState, Enemy, EnemyState) is Victor victor)
            {
                return victor;
            }
            if (playerState.Debuffs.Contains(Debuff.Burning))
            {
                LogLine($"**{player.Name}** burns!");
                playerState.Health -= player.HealthMax / 12;
                if (playerState.Health <= 0)
                {
                    LogLine($"**{player.Name}** falls!");
                    if (Players.All(pair => pair.Value.Health <= 0))
                    {
                        return Victor.Enemies;
                    }
                }
            }
        }
        return null;
    }

    public Victor? TurnEnemy()
    {
        return EnemyAttacks(Players, Enemy, EnemyState);
    }

    public Victor? PlayerAttacks(IPlayer attacker, PlayerState attackerState, IEnemy target, EnemyState targetState)
    {
        if (attackerState.Debuffs.Contains(Debuff.Subjugation))
        {
            LogLine($"**{attacker.Name}** couldn't attack due to **Subjugation**!");
            return null;
        }
        LogLine($"**{attacker.Name}** strikes with {attacker.Weapon.Name}!");
        int damage = VariateDamage(attacker.Weapon.DealDamage(attacker, target));
        if (attackerState.Debuffs.Contains(Debuff.Weakened))
        {
            damage = Convert.ToInt32(damage * 0.75);
        }

        targetState.Health -= damage;
        if (targetState.Health <= 0)
        {
            LogLine($"**{target.Name}** falls!");
            return Victor.Players;
        }
        return null;
    }

    public Victor? EnemyAttacks(IReadOnlyDictionary<IPlayer, PlayerState> targets, IEnemy attacker, EnemyState attackerState)
    {
        IReadOnlyDictionary<IPlayer, EnemyHit> hitResults = attacker.Attack(attackerState, targets);
        foreach (KeyValuePair<IPlayer, EnemyHit> hitResult in hitResults)
        {
            if (targets[hitResult.Key].Health <= 0)
            {
                continue;
            }
            LogLine($"**{attacker.Name}** strikes **{hitResult.Key.Name}** with **{hitResult.Value.AttackName}**!");

            int damage = VariateDamage(hitResult.Value.Damage);
            if (targets[hitResult.Key].Debuffs.Contains(Debuff.Weakened))
            {
                damage = Convert.ToInt32(damage * 1.25);
            }

            targets[hitResult.Key].Health -= damage;
            targets[hitResult.Key].Debuffs.UnionWith(hitResult.Value.Debuffs);
            if (targets[hitResult.Key].Health <= 0)
            {
                LogLine($"**{hitResult.Key.Name}** falls!");
                if (Players.All(pair => pair.Value.Health <= 0))
                {
                    return Victor.Enemies;
                }
            }
        }
        return null;
    }

    [Pure]
    private int VariateDamage(int incomingDamage)
    {
        double multiplier = 0.875 + Random.Shared.NextDouble() / 4;
        return Convert.ToInt32(incomingDamage * multiplier);
    }

    [Pure]
    private int VariateCredits(int credits)
    {
        double multiplier = 0.9 + Random.Shared.NextDouble() / 5;
        return Convert.ToInt32(credits * multiplier);
    }
}
