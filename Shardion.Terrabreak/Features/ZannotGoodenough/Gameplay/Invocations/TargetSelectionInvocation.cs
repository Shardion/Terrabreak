using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

public record TargetSelectionInvocation(
    Battle Battlefield,
    BattleLoadout AttackingPlayer,
    BattleLoadout DefendingPlayer,
    BattleMonster Attacker
);
