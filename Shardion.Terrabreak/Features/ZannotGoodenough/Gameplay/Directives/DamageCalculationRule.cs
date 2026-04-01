using System;
using System.Collections.Generic;
using Serilog;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;

public record DamageCalculationRule(DirectiveSource Source, DamageCalculationInvocation Invocation) : IBattleRule<DamageCalculationResult, DamageCalculationInvocation>
{
    public string? LogLine => null;

    public IEnumerable<DamageCalculationInvocation> FireHooks()
    {
        List<DamageCalculationInvocation> invocations = [];
        foreach (BattleRelic friendlyRelic in Invocation.AttackingPlayer.GetRelicEnumerator())
        {
            if (friendlyRelic.Relic.HookDamageCalculation(Invocation, friendlyRelic, friendlyRelic.Relic.CastState(friendlyRelic.State)) is DamageCalculationInvocation hookInvocation)
            {
                invocations.Add(hookInvocation);
            }
        }
        foreach (BattleRelic enemyRelic in Invocation.DefendingPlayer.GetRelicEnumerator())
        {
            if (enemyRelic.Relic.HookDamageCalculation(Invocation, enemyRelic, enemyRelic.Relic.CastState(enemyRelic.State)) is DamageCalculationInvocation hookInvocation)
            {
                invocations.Add(hookInvocation);
            }
        }

        return invocations;
    }

    public IEnumerable<DamageCalculationResult> FireInterceptors()
    {
        List<DamageCalculationResult> rules = [];
        foreach (BattleRelic friendlyRelic in Invocation.AttackingPlayer.GetRelicEnumerator())
        {
            if (friendlyRelic.Relic.InterceptDamageCalculation(Invocation, friendlyRelic, friendlyRelic.Relic.CastState(friendlyRelic.State)) is DamageCalculationResult interceptorRules)
            {
                rules.Add(interceptorRules);
            }
        }
        foreach (BattleRelic enemyRelic in Invocation.DefendingPlayer.GetRelicEnumerator())
        {
            if (enemyRelic.Relic.InterceptDamageCalculation(Invocation, enemyRelic, enemyRelic.Relic.CastState(enemyRelic.State)) is DamageCalculationResult interceptorRules)
            {
                rules.Add(interceptorRules);
            }
        }

        return rules;
    }

    public DamageCalculationResult Execute()
    {
        int attack = Convert.ToInt32(Math.Round(
            (Invocation.Attack + Invocation.AttackStaticModifier) *
            ((Invocation.AttackPercentageModifier + 100) * 0.01),
            MidpointRounding.AwayFromZero));
        int defense = Convert.ToInt32(Math.Round(
            (Invocation.Defense + Invocation.DefenseStaticModifier) *
            ((Invocation.DefensePercentageModifier + 100) * 0.01),
            MidpointRounding.ToZero));
        Log.Debug("Calculated final damage {final} with ATK {attack} and DEF {defense}.",
            Math.Min(Invocation.Defender.State.CurrentHealth, Math.Max(1, attack - defense)),
            attack,
            defense
        );
        return new([],
            Math.Min(Invocation.Defender.State.CurrentHealth, Math.Max(1, attack - defense)));
    }

    public IBattleRule<DamageCalculationResult, DamageCalculationInvocation> Modify(object maybeInvocation)
    {
        if (maybeInvocation is not DamageCalculationInvocation invocation)
        {
            return this;
        }
        return this with { Invocation = invocation };
    }
}


public record DamageCalculationResult(IEnumerable<IBattleDirective> Directives, int FinalDamage) : IRuleResult
{
    public string? LogLine => null;
    public IEnumerable<IBattleDirective> Directives { get; } = Directives;
}
