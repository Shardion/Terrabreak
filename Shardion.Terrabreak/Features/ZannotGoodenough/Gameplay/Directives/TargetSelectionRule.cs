using System;
using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;

public record TargetSelectionRule(DirectiveSource Source, TargetSelectionInvocation Invocation) : IBattleRule<TargetSelectionResult, TargetSelectionInvocation>
{
    public string? LogLine => null;

    public IEnumerable<TargetSelectionInvocation> FireHooks()
    {
        return [];
    }

    public IEnumerable<TargetSelectionResult> FireInterceptors()
    {
        List<TargetSelectionResult> rules = [];
        foreach (BattleRelic friendlyRelic in Invocation.AttackingPlayer.GetRelicEnumerator())
        {
            if (friendlyRelic.Relic.InterceptTargetSelection(Invocation, friendlyRelic, friendlyRelic.Relic.CastState(friendlyRelic.State)) is TargetSelectionResult interceptorRules)
            {
                rules.Add(interceptorRules);
            }
        }
        foreach (BattleRelic enemyRelic in Invocation.DefendingPlayer.GetRelicEnumerator())
        {
            if (enemyRelic.Relic.InterceptTargetSelection(Invocation, enemyRelic, enemyRelic.Relic.CastState(enemyRelic.State)) is TargetSelectionResult interceptorRules)
            {
                rules.Add(interceptorRules);
            }
        }

        return rules;
    }

    public TargetSelectionResult Execute()
    {
        BattleMonster? randomTarget = Invocation.DefendingPlayer.GetMonsterEnumerator()
            .Where(monster => !BattleRules.CheckKnockout(monster))
            .Shuffle()
            .FirstOrDefault();
        return new([], randomTarget);
    }

    public IBattleRule<TargetSelectionResult, TargetSelectionInvocation> Modify(object maybeInvocation)
    {
        if (maybeInvocation is not TargetSelectionInvocation invocation)
        {
            return this;
        }
        return this with { Invocation = invocation };
    }
}

public record TargetSelectionResult(IEnumerable<IBattleDirective> Directives, BattleMonster? Target) : IRuleResult
{
    public string? LogLine => null;
    public IEnumerable<IBattleDirective> Directives { get; } = Directives;
}
