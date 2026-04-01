using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

public record HealInvocation(
    Battle Battlefield,
    BattleLoadout HealingPlayer,
    BattleLoadout OppositePlayer,
    BattleMonster Healer,
    BattleMonster Receiver,
    int BaseHealing,
    int HealingPercentageBoost,
    int HealingStaticBoost
);
