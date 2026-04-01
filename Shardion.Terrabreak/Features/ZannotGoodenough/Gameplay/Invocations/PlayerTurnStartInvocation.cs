using System.Collections.Generic;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

public record PlayerTurnStartInvocation(
    Battle Battlefield,
    BattleLoadout FriendlyPlayer,
    BattleLoadout EnemyPlayer
);
