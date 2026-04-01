using System;
using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Spirit;

public class MisterBones : IMonster<MisterBonesState>
{
    public string Name => "Mister Bones";
    public string Description => "This particular skeleton was reanimated by the Necromancer Tomb From Within The Moon.";
    public MonsterClassification Classification => MonsterClassification.Spirit;
    public MonsterCharacteristic Characteristic => MonsterCharacteristic.Blunt;

    public int BaseHealth => 32;
    public int BaseAttack => 2;
    public int BaseDefense => 1;

    public ILikesAbility<MisterBonesState> LikesAbility { get; } = new LikesAbilitySkeletonWar();

    public DamageCalculationInvocation? HookDamageCalculation(DamageCalculationInvocation invocation, BattleMonster thisMonster,
        MonsterState thisState)
    {
        if (invocation.Defender != thisMonster)
        {
            return null;
        }
        MisterBonesState state = (MisterBonesState)thisState;
        if (state.DefAddTurnsRemaining > 0)
        {
            return invocation with { DefenseStaticModifier = invocation.DefenseStaticModifier + 2 };
        }

        return null;
    }

    public IEnumerable<IBattleDirective>? HookPlayerTurnStart(TurnStartInvocation invocation, BattleMonster thisMonster, MonsterState thisState)
    {
        MisterBonesState state = (MisterBonesState)thisState;
        state.DefAddTurnsRemaining = Math.Max(0, state.DefAddTurnsRemaining - 1);
        return null;
    }
}

public class MisterBonesState : MonsterState
{
    public int DefAddTurnsRemaining { get; set; } = 0;
}

public class LikesAbilitySkeletonWar : ILikesAbility<MisterBonesState>
{
    public string Name => "Skeleton War";
    public string Description => "For 4 turns, DEF increases by 2.";
    // Also instantly KOs all Mister Bones if there is at least one on both sides of the battle
    public int BaseLikesCost => 4;

    public IEnumerable<IBattleDirective>? Execute(LikesAbilityInvocation<MonsterState> arguments)
    {
        MisterBonesState state = (MisterBonesState)arguments.UserState;
        state.DefAddTurnsRemaining += 4;
        return null;
    }
}
