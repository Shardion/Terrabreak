using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

public record DamageInvocation(
    Battle Battlefield,
    BattleLoadout DefendingPlayer,
    BattleMonster Defender,
    int FinalDamage
);
