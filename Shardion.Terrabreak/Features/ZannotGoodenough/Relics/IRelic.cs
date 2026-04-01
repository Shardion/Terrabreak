using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics;

public interface IRelic<out TState> : INamedEntity where TState: notnull, new()
{
    public IRelicSeries Series { get; }
    public string EffectDescription { get; }
    public IEnumerable<RelicDomainPart> Domain { get; }
    public IEnumerable<RelicCategory> Conflicts => [];

    public TState NewState()
    {
        return new();
    }

    public TState CastState(RelicState state)
    {
        if (state is not TState castedState)
        {
            throw new InvalidCastException($"Cannot cast to {GetType().Name}'s state with a non-{nameof(TState)} state type.");
        }
        return castedState;
    }

    public IEnumerable<IBattleDirective>? HookBattleStart(BattleStartInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        return null;
    }

    public IEnumerable<IBattleDirective>? HookTurnStart(TurnStartInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        return null;
    }

    public IEnumerable<IBattleDirective>? HookPlayerTurnStart(PlayerTurnStartInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        return null;
    }

    public IEnumerable<IBattleDirective>? HookAttack(AttackInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        return null;
    }

    public IEnumerable<IBattleDirective>? HookHeal(HealInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        return null;
    }

    public IEnumerable<IBattleDirective>? InterceptHeal(HealInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        return null;
    }

    public DamageCalculationResult? InterceptDamageCalculation(DamageCalculationInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        return null;
    }

    public DamageCalculationInvocation? HookDamageCalculation(DamageCalculationInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        return null;
    }

    public LikesCostInvocation<MonsterState>? HookLikesCost(LikesCostInvocation<MonsterState> invocation, BattleRelic thisRelic, RelicState thisState)
    {
        return null;
    }

    public DodgingResult? InterceptDodge(DodgingInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        return null;
    }

    public TargetSelectionResult? InterceptTargetSelection(TargetSelectionInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        return null;
    }

    public IEnumerable<IBattleDirective>? InterceptGrantLikes(GrantLikesInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        return null;
    }

    public IEnumerable<IBattleDirective>? InterceptBasicAttack(BasicAttackInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        return null;
    }

    public IEnumerable<IBattleDirective>? InterceptLikesAbility(LikesAbilityInvocation<MonsterState> invocation, BattleRelic thisRelic, RelicState thisState)
    {
        return null;
    }
}
