using System;
using System.Collections.Generic;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Services.Emoji;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

public interface IMonster<out TState> : INamedEntity where TState: MonsterState, new()
{
    public TState NewState()
    {
        return new();
    }

    public TState CastState(MonsterState state)
    {
        if (state is not TState castedState)
        {
            throw new InvalidCastException($"Cannot cast to {GetType().Name}'s state with a non-{nameof(TState)} state type.");
        }
        return castedState;
    }

    public int BaseHealth { get; }
    public int BaseAttack { get; }
    public int BaseDefense { get; }

    public MonsterClassification Classification { get; }
    public MonsterCharacteristic Characteristic { get; }

    public ILikesAbility<TState> LikesAbility { get; }

    public bool Hidden => false;

    public static ManagedEmoji ProduceClassificationIcon(EmojiManager emojiManager, IMonster<MonsterState> monster) =>
        monster.Classification switch
        {
            MonsterClassification.Rodent => emojiManager.GetEmoji("rodent"),
            MonsterClassification.Nature => emojiManager.GetEmoji("nature"),
            MonsterClassification.Machina => emojiManager.GetEmoji("machina"),
            MonsterClassification.Spirit => emojiManager.GetEmoji("spirit"),
            _ => throw new ArgumentOutOfRangeException()
        };

    public DodgingResult? InterceptDodge(DodgingInvocation invocation, BattleMonster thisMonster, MonsterState thisState)
    {
        return null;
    }

    public DamageCalculationInvocation? HookDamageCalculation(DamageCalculationInvocation invocation, BattleMonster thisMonster, MonsterState thisState)
    {
        return null;
    }

    public IEnumerable<IBattleDirective>? InterceptAttack(AttackInvocation invocation, BattleMonster thisMonster, MonsterState thisState)
    {
        return null;
    }

    public IEnumerable<IBattleDirective>? HookTurnStart(TurnStartInvocation invocation, BattleMonster thisMonster,
        MonsterState thisState)
    {
        return null;
    }

    public IEnumerable<IBattleDirective>? HookPlayerTurnStart(PlayerTurnStartInvocation invocation, BattleMonster thisMonster,
        MonsterState thisState)
    {
        return null;
    }
}
