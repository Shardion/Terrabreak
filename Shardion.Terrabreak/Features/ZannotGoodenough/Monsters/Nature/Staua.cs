using System.Collections.Generic;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Nature;

public class Staua : IMonster<StauaState>
{
    public string Name => "Staua";
    public string Description => "A species of pig commonly found near high-altitude marble deposits.";
    public int BaseHealth => 28;
    public int BaseAttack => 3;
    public int BaseDefense => 0;

    public MonsterClassification Classification => MonsterClassification.Nature;
    public MonsterCharacteristic Characteristic => MonsterCharacteristic.Piercing;
    public ILikesAbility<StauaState> LikesAbility => new MarbleBoxLikesAbility();

    public DodgingResult? InterceptDodge(DodgingInvocation invocation, BattleMonster thisMonster, MonsterState thisState)
    {
        if (invocation.Defender == thisMonster)
        {
            StauaState state = (StauaState)thisState;
            if (state.DodgesRemaining > 0)
            {
                state.DodgesRemaining -= 1;
                return new([], true);
            }
        }

        return null;
    }

    public DamageCalculationInvocation? HookDamageCalculation(DamageCalculationInvocation invocation, BattleMonster thisMonster,
        MonsterState thisState)
    {
        if (invocation.Attacker == thisMonster)
        {
            StauaState state = (StauaState)thisState;
            if (state.DodgesRemaining > 0)
            {
                return invocation with { Attack = 0, AttackStaticModifier = 0, AttackPercentageModifier = 0 };
            }
        }

        return null;
    }
}

public class StauaState : MonsterState
{
    public int DodgesRemaining { get; set; } = 0;
}

public class MarbleBoxLikesAbility : ILikesAbility<StauaState>
{
    public string Name => "Marble Box";
    public string Description => "Dodge the next two incoming attacks, but ATK is 0 while dodges remain.";
    public int BaseLikesCost => 5;

    public IEnumerable<IBattleDirective>? Execute(LikesAbilityInvocation<MonsterState> arguments)
    {
        StauaState state = (StauaState)arguments.UserState;
        state.DodgesRemaining += 2;

        return null;
    }
}
