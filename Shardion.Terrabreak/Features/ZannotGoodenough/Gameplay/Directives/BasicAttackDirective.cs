using System.Collections.Generic;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;

public record BasicAttackDirective(DirectiveSource Source, BasicAttackInvocation Invocation) : IBattleDirective
{
    public string? LogLine => null;

    public IEnumerable<IBattleDirective>? FireHooks()
    {
        return null;
    }

    public IEnumerable<IBattleDirective>? FireInterceptors()
    {
        List<IBattleDirective> directives = [];
        foreach (BattleRelic friendlyRelic in Invocation.AttackingPlayer.GetRelicEnumerator())
        {
            if (friendlyRelic.Relic.InterceptBasicAttack(Invocation, friendlyRelic, friendlyRelic.Relic.CastState(friendlyRelic.State)) is IEnumerable<IBattleDirective> hookDirectives)
            {
                directives.AddRange(hookDirectives);
            }
        }
        foreach (BattleRelic enemyRelic in Invocation.DefendingPlayer.GetRelicEnumerator())
        {
            if (enemyRelic.Relic.InterceptBasicAttack(Invocation, enemyRelic, enemyRelic.Relic.CastState(enemyRelic.State)) is IEnumerable<IBattleDirective> hookDirectives)
            {
                directives.AddRange(hookDirectives);
            }
        }
        return directives;
    }

    public IEnumerable<IBattleDirective>? Execute()
    {
        DamageCalculationResult result = Battle.RunRules(new DamageCalculationRule(
            Source,
            new(
                Invocation.Battlefield,
                Invocation.AttackingPlayer,
                Invocation.DefendingPlayer,
                Invocation.Attacker,
                Invocation.Defender,
                Invocation.Attacker.Monster.BaseAttack,
                Invocation.Attacker.State.AttackStaticModifier,
                Invocation.Attacker.State.AttackPercentageModifier,
                Invocation.Defender.Monster.BaseDefense,
                Invocation.Defender.State.DefenseStaticModifier,
                Invocation.Defender.State.DefensePercentageModifier
            )));
        return
        [
            new AttackDirective(Source,
                new(
                    Invocation.Battlefield,
                    Invocation.AttackingPlayer,
                    Invocation.DefendingPlayer,
                    Invocation.Attacker,
                    Invocation.Defender,
                    result.FinalDamage,
                    0, // TODO
                    0
                )
            ),
        ];
    }
}
