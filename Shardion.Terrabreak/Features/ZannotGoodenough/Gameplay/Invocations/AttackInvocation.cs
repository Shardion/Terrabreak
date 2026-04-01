using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

public record AttackInvocation(
    Battle Battlefield,
    BattleLoadout AttackingPlayer,
    BattleLoadout DefendingPlayer,
    BattleMonster Attacker,
    BattleMonster Defender,
    int BaseFinalDamage,
    int FinalDamagePercentageBoost,
    int FinalDamageStaticBoost
);
