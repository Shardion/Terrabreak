using System;
using System.Collections.Generic;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;

public record LikesCostRule(DirectiveSource Source, LikesCostInvocation<MonsterState> Invocation)
    : IBattleRule<LikesCostResult, LikesCostInvocation<MonsterState>>
{
    public IEnumerable<LikesCostInvocation<MonsterState>> FireHooks()
    {
        List<LikesCostInvocation<MonsterState>> invocations = [];
        foreach (BattleRelic friendlyRelic in Invocation.FriendlyPlayer.GetRelicEnumerator())
        {
            if (friendlyRelic.Relic.HookLikesCost(Invocation, friendlyRelic, friendlyRelic.Relic.CastState(friendlyRelic.State)) is LikesCostInvocation<MonsterState> hookInvocation)
            {
                invocations.Add(hookInvocation);
            }
        }
        foreach (BattleRelic enemyRelic in Invocation.EnemyPlayer.GetRelicEnumerator())
        {
            if (enemyRelic.Relic.HookLikesCost(Invocation, enemyRelic, enemyRelic.Relic.CastState(enemyRelic.State)) is LikesCostInvocation<MonsterState> hookInvocation)
            {
                invocations.Add(hookInvocation);
            }
        }

        return invocations;
    }

    public IEnumerable<LikesCostResult> FireInterceptors()
    {
        return [];
    }

    public LikesCostResult Execute()
    {
        return new([],
            Convert.ToInt32(Math.Round(
            (Invocation.BaseLikesCost + Invocation.LikesCostStaticModifier) *
            ((Invocation.LikesCostPercentageModifier + 100) * 0.01),
            MidpointRounding.ToZero)));
    }

    public IBattleRule<LikesCostResult, LikesCostInvocation<MonsterState>> Modify(object maybeInvocation)
    {
        if (maybeInvocation is not LikesCostInvocation<MonsterState> invocation)
        {
            return this;
        }
        return this with { Invocation = invocation };
    }
}

public record LikesCostResult(IEnumerable<IBattleDirective> Directives, int FinalLikesCost) : IRuleResult
{
    public IEnumerable<IBattleDirective> Directives { get; } = Directives;
}
