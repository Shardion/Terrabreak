namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

public record DamageCalculationInvocation(
    Battle Battlefield,
    BattleLoadout AttackingPlayer,
    BattleLoadout DefendingPlayer,
    BattleMonster Attacker,
    BattleMonster Defender,
    int Attack,
    int AttackStaticModifier,
    int AttackPercentageModifier,
    int Defense,
    int DefenseStaticModifier,
    int DefensePercentageModifier
);
