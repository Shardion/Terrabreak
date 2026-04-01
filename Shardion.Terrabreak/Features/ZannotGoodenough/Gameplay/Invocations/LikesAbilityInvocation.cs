using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

public record LikesAbilityInvocation<TUserState>(
    Battle Battlefield,
    BattleLoadout FriendlyPlayer,
    BattleLoadout EnemyPlayer,
    BattleMonster User,
    TUserState UserState
);
