using System;
using System.Collections.Generic;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;

public record HealDirective(DirectiveSource Source, HealInvocation Invocation) : IBattleDirective
{
    public string? LogLine => null;

    public IEnumerable<IBattleDirective>? FireHooks()
    {
        List<IBattleDirective> directives = [];
        foreach (BattleRelic friendlyRelic in Invocation.HealingPlayer.GetRelicEnumerator())
        {
            if (friendlyRelic.Relic.HookHeal(Invocation, friendlyRelic, friendlyRelic.Relic.CastState(friendlyRelic.State)) is IEnumerable<IBattleDirective> hookDirectives)
            {
                directives.AddRange(hookDirectives);
            }
        }
        foreach (BattleRelic enemyRelic in Invocation.OppositePlayer.GetRelicEnumerator())
        {
            if (enemyRelic.Relic.HookHeal(Invocation, enemyRelic, enemyRelic.Relic.CastState(enemyRelic.State)) is IEnumerable<IBattleDirective> hookDirectives)
            {
                directives.AddRange(hookDirectives);
            }
        }

        return directives;
    }

    public IEnumerable<IBattleDirective>? FireInterceptors()
    {
        List<IBattleDirective> directives = [];
        foreach (BattleRelic healingRelic in Invocation.HealingPlayer.GetRelicEnumerator())
        {
            if (healingRelic.Relic.InterceptHeal(Invocation, healingRelic, healingRelic.Relic.CastState(healingRelic.State)) is IEnumerable<IBattleDirective> interceptorDirectives)
            {
                directives.AddRange(interceptorDirectives);
            }
        }
        foreach (BattleRelic oppositeRelic in Invocation.OppositePlayer.GetRelicEnumerator())
        {
            if (oppositeRelic.Relic.InterceptHeal(Invocation, oppositeRelic, oppositeRelic.Relic.CastState(oppositeRelic.State)) is IEnumerable<IBattleDirective> interceptorDirectives)
            {
                directives.AddRange(interceptorDirectives);
            }
        }
        return directives;
    }

    public IEnumerable<IBattleDirective>? Execute()
    {
        if (BattleRules.CheckKnockout(Invocation.Receiver))
        {
            return null;
        }
        int healing = Convert.ToInt32(Math.Round(
            (Invocation.BaseHealing + Invocation.HealingStaticBoost) *
            ((Invocation.HealingPercentageBoost + 100) * 0.01),
            MidpointRounding.AwayFromZero));
        Invocation.Receiver.State.CurrentHealth = Math.Min(Invocation.Receiver.State.MaxHealth, Invocation.Receiver.State.CurrentHealth + healing);
        return [];
    }
}
