using System;
using System.Collections.Generic;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Rodent;

public class PlagueMaster : IMonster<PlagueMasterState>
{
    public string Name => "Plague Master";

    public string Description =>
        "This terrible rodent hovers within the diseased gases it constantly emits, which infect any living thing they touch.";
    public int BaseHealth => 18;
    public int BaseAttack => 5;
    public int BaseDefense => 1;
    public MonsterClassification Classification => MonsterClassification.Rodent;
    public MonsterCharacteristic Characteristic => MonsterCharacteristic.Haunting;
    public ILikesAbility<PlagueMasterState> LikesAbility => new LikesAbilityVileSmog();

    public DamageCalculationInvocation? HookDamageCalculation(DamageCalculationInvocation invocation, BattleMonster thisMonster,
        MonsterState thisState)
    {
        PlagueMasterState state = (PlagueMasterState)thisState;
        if (state.DefZeroTurnsRemaining > 0 && invocation.Defender == thisMonster)
        {
            return invocation with
            {
                Defense = 0,
                DefenseStaticModifier = 0,
                DefensePercentageModifier = 0,
                AttackPercentageModifier = invocation.AttackPercentageModifier - 50,
            };
        }

        return null;
    }

    public IEnumerable<IBattleDirective>? HookPlayerTurnStart(PlayerTurnStartInvocation invocation, BattleMonster thisMonster, MonsterState thisState)
    {
        PlagueMasterState state = (PlagueMasterState)thisState;
        state.DefZeroTurnsRemaining = Math.Max(0, state.DefZeroTurnsRemaining - 1);
        return null;
    }
}

public class PlagueMasterState : MonsterState
{
    public int DefZeroTurnsRemaining { get; set; } = 0;
}

public class LikesAbilityVileSmog : ILikesAbility<PlagueMasterState>
{
    public string Name => "Vile Smog";
    public string Description => "For 3 turns, DEF is 0, but incoming damage is -50%.";
    public int BaseLikesCost => 6;

    public IEnumerable<IBattleDirective>? Execute(LikesAbilityInvocation<MonsterState> arguments)
    {
        PlagueMasterState state = (PlagueMasterState)arguments.UserState;
        state.DefZeroTurnsRemaining += 3;
        return null;
    }
}
