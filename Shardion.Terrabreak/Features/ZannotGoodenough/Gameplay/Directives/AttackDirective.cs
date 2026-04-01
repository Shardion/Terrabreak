using System;
using System.Collections.Generic;
using Serilog;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;

public record AttackDirective(DirectiveSource Source, AttackInvocation Invocation, string? LogLine = null) : IBattleDirective
{
    public string? LogLine { get; private set; } = LogLine;

    public IEnumerable<IBattleDirective>? FireHooks()
    {
        List<IBattleDirective> directives = [];
        foreach (BattleRelic friendlyRelic in Invocation.AttackingPlayer.GetRelicEnumerator())
        {
            if (friendlyRelic.Relic.HookAttack(Invocation, friendlyRelic, friendlyRelic.Relic.CastState(friendlyRelic.State)) is IEnumerable<IBattleDirective> hookDirectives)
            {
                directives.AddRange(hookDirectives);
            }
        }
        foreach (BattleRelic enemyRelic in Invocation.DefendingPlayer.GetRelicEnumerator())
        {
            if (enemyRelic.Relic.HookAttack(Invocation, enemyRelic, enemyRelic.Relic.CastState(enemyRelic.State)) is IEnumerable<IBattleDirective> hookDirectives)
            {
                directives.AddRange(hookDirectives);
            }
        }

        return directives;
    }

    public IEnumerable<IBattleDirective>? FireInterceptors()
    {
        List<IBattleDirective> directives = [];
        foreach (BattleMonster friendlyMonster in Invocation.AttackingPlayer.GetMonsterEnumerator())
        {
            if (friendlyMonster.Monster.InterceptAttack(Invocation, friendlyMonster, friendlyMonster.Monster.CastState(friendlyMonster.State)) is IEnumerable<IBattleDirective> hookDirectives)
            {
                directives.AddRange(hookDirectives);
            }
        }
        foreach (BattleMonster enemyMonster in Invocation.DefendingPlayer.GetMonsterEnumerator())
        {
            if (enemyMonster.Monster.InterceptAttack(Invocation, enemyMonster, enemyMonster.Monster.CastState(enemyMonster.State)) is IEnumerable<IBattleDirective> hookDirectives)
            {
                directives.AddRange(hookDirectives);
            }
        }
        return directives;
    }

    public IEnumerable<IBattleDirective>? Execute()
    {
        DodgingResult result = Battle.RunRules(new DodgingRule(
                Source,
                new(Invocation.Battlefield,
                    Invocation.AttackingPlayer,
                    Invocation.DefendingPlayer,
                    Invocation.Attacker,
                    Invocation.Defender,
                    false,
                    Invocation.Defender.State.DodgeChance
                )
            )
        );

        if (result.AttackDodged)
        {
            LogLine = $"{Invocation.DefendingPlayer.Player.Name}'s {Invocation.Defender.Monster.Name} dodges {Invocation.Attacker.Monster.Name}'s attack!";
            return null;
        }

        int damage = Convert.ToInt32(Math.Round(
            (Invocation.BaseFinalDamage + Invocation.FinalDamageStaticBoost) *
            ((Invocation.FinalDamagePercentageBoost + 100) * 0.01),
            MidpointRounding.AwayFromZero));
        Log.Debug("Attack executed by {monster} dealing {damage} damage.", Invocation.Attacker.Monster.Name, damage);
        Invocation.Defender.State.CurrentHealth = Math.Max(0, Invocation.Defender.State.CurrentHealth - damage);

        return
        [
            new GrantLikesDirective(
                Source,
                new(Invocation.Battlefield, Invocation.AttackingPlayer, 1)
            ),
        ];
    }
}
