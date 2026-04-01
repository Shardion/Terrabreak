using System.Collections.Generic;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Machina;

public class RazRat : IMonster<RazRatState>
{
    public string Name => "Raz Rat";
    public string Description => "Raz Rat wasn't always the ruler of the rats. He was originally an animatronic meant for children's birthday parties.";
    public MonsterClassification Classification => MonsterClassification.Machina;
    public MonsterCharacteristic Characteristic => MonsterCharacteristic.Piercing;

    public int BaseHealth => 20;
    public int BaseAttack => 3;
    public int BaseDefense => 0;

    public ILikesAbility<RazRatState> LikesAbility { get; } = new LikesAbilitySilentApproach();

    public RazRatState NewState()
    {
        return new();
    }

    public DodgingResult? InterceptDodge(DodgingInvocation invocation, BattleMonster thisMonster, MonsterState thisState)
    {
        if (invocation.Defender.State == thisState)
        {
            RazRatState state = (RazRatState)thisState;
            if (state.DodgesRemaining > 0)
            {
                state.DodgesRemaining -= 1;
                return new([], true);
            }
        }

        return null;
    }
}

public class RazRatState : MonsterState
{
    public int DodgesRemaining { get; set; } = 0;
}

public class LikesAbilitySilentApproach : ILikesAbility<RazRatState>
{
    public string Name => "Silent Approach";
    public string Description => "Dodges the next incoming attack.";
    public int BaseLikesCost => 4;

    public IEnumerable<IBattleDirective>? Execute(LikesAbilityInvocation<MonsterState> arguments)
    {
        RazRatState state = (RazRatState)arguments.UserState;
        state.DodgesRemaining += 1;

        return null;
    }
}
