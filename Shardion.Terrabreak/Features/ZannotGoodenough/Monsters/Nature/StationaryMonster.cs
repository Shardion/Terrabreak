using System;
using System.Collections.Generic;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Nature;

public class StationaryMonster : IMonster<StationaryMonsterState>
{
    public string Name => "Stationary Monster";
    public string Description => "It's unknown how Stationary Monsters survive without moving. Some suspect they generate energy using the laws of physics.";
    public int BaseHealth => 22;
    public int BaseAttack => 2;
    public int BaseDefense => 2;
    public MonsterClassification Classification => MonsterClassification.Nature;
    public MonsterCharacteristic Characteristic => MonsterCharacteristic.Blunt;
    public ILikesAbility<StationaryMonsterState> LikesAbility => new FortifyLikesAbility();

    public IEnumerable<IBattleDirective>? InterceptAttack(AttackInvocation invocation, BattleMonster thisMonster, MonsterState thisState)
    {
        if (invocation.Defender == thisMonster)
        {
            StationaryMonsterState state = (StationaryMonsterState)thisState;
            if (state.FortifyTurnsRemaining > 0)
            {
                return
                [
                    new DamageDirective(
                        new(this, null, thisMonster),
                        new(invocation.Battlefield, invocation.AttackingPlayer, invocation.Attacker, 2),
                        $"{invocation.AttackingPlayer.Player.Name}'s {invocation.Attacker.Monster.Name} crashes against {thisMonster.Monster.Name}!"
                    )
                ];
            }
        }

        return null;
    }

    public IEnumerable<IBattleDirective>? HookPlayerTurnStart(PlayerTurnStartInvocation invocation, BattleMonster thisMonster, MonsterState thisState)
    {
        StationaryMonsterState state = (StationaryMonsterState)thisState;
        state.FortifyTurnsRemaining = Math.Max(0, state.FortifyTurnsRemaining - 1);
        return null;
    }
}

public class StationaryMonsterState : MonsterState
{
    public int FortifyTurnsRemaining { get; set; } = 0;
}

public class FortifyLikesAbility : ILikesAbility<StationaryMonsterState>
{
    public string Name => "Fortify";
    public string Description => "For one turn, incoming attacks are negated, and attackers take a small amount of damage.";
    public int BaseLikesCost => 7;

    public IEnumerable<IBattleDirective>? Execute(LikesAbilityInvocation<MonsterState> arguments)
    {
        StationaryMonsterState state = (StationaryMonsterState)arguments.UserState;
        state.FortifyTurnsRemaining += 1;
        return null;
    }
}
