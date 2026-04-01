using System;
using System.Collections.Generic;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

public interface ILikesAbility<out TMonsterState> : INamedEntity where TMonsterState : MonsterState, new()
{
    public int BaseLikesCost { get; }
    protected IEnumerable<IBattleDirective>? Execute(LikesAbilityInvocation<MonsterState> arguments);

    public IEnumerable<IBattleDirective>? ExecuteCastingState(LikesAbilityInvocation<MonsterState> arguments)
    {
        if (arguments.User.State is not TMonsterState state)
        {
            throw new InvalidCastException($"Cannot execute a likes ability {GetType().Name} with a non-{nameof(TMonsterState)} state type.");
        }
        return Execute(arguments);
    }
}
