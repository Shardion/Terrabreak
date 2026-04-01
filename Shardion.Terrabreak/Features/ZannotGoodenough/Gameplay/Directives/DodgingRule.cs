using System;
using System.Collections.Generic;
using Serilog;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;

public record DodgingRule(DirectiveSource Source, DodgingInvocation Invocation) : IBattleRule<DodgingResult, DodgingInvocation>
{
    public IEnumerable<DodgingInvocation> FireHooks()
    {
        return [];
    }

    public IEnumerable<DodgingResult> FireInterceptors()
    {
        List<DodgingResult> rules = [];
        foreach (BattleMonster friendlyMonster in Invocation.AttackingPlayer.GetMonsterEnumerator())
        {
            if (friendlyMonster.Monster.InterceptDodge(Invocation, friendlyMonster, friendlyMonster.Monster.CastState(friendlyMonster.State)) is DodgingResult interceptorRules)
            {
                rules.Add(interceptorRules);
            }
        }
        foreach (BattleMonster enemyMonster in Invocation.DefendingPlayer.GetMonsterEnumerator())
        {
            if (enemyMonster.Monster.InterceptDodge(Invocation, enemyMonster, enemyMonster.Monster.CastState(enemyMonster.State)) is DodgingResult interceptorRules)
            {
                rules.Add(interceptorRules);
            }
        }
        foreach (BattleRelic friendlyRelic in Invocation.AttackingPlayer.GetRelicEnumerator())
        {
            if (friendlyRelic.Relic.InterceptDodge(Invocation, friendlyRelic, friendlyRelic.Relic.CastState(friendlyRelic.State)) is DodgingResult interceptorRules)
            {
                rules.Add(interceptorRules);
            }
        }
        foreach (BattleRelic enemyRelic in Invocation.DefendingPlayer.GetRelicEnumerator())
        {
            if (enemyRelic.Relic.InterceptDodge(Invocation, enemyRelic, enemyRelic.Relic.CastState(enemyRelic.State)) is DodgingResult interceptorRules)
            {
                rules.Add(interceptorRules);
            }
        }

        return rules;
    }

    public DodgingResult Execute()
    {
        if (Invocation.DodgeGuaranteed)
        {
            Log.Debug("{monster} dodged with guarantee.", Invocation.Defender.Monster.Name);
            return new([], true);
        }

        double rng = Random.Shared.NextDouble();

        if (rng <= Invocation.DodgeChance)
        {
            Log.Debug("{monster} dodged with {rng}/{chance}.", Invocation.Defender.Monster.Name, rng, Invocation.DodgeChance);
            return new([], true);
        }

        Log.Debug("{monster} failed to dodge with {rng}/{chance}.", Invocation.Defender.Monster.Name, rng, Invocation.DodgeChance);
        return new([], false);
    }

    public IBattleRule<DodgingResult, DodgingInvocation> Modify(object maybeInvocation)
    {
        if (maybeInvocation is not DodgingInvocation invocation)
        {
            return this;
        }
        return this with { Invocation = invocation };
    }
}

public record DodgingResult(IEnumerable<IBattleDirective> Directives, bool AttackDodged) : IRuleResult
{
    public IEnumerable<IBattleDirective> Directives { get; } = Directives;
}
