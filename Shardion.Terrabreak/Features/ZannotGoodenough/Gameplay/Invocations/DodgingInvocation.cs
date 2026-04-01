namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

public record DodgingInvocation(
    Battle Battlefield,
    BattleLoadout AttackingPlayer,
    BattleLoadout DefendingPlayer,
    BattleMonster Attacker,
    BattleMonster Defender,
    bool DodgeGuaranteed,
    double DodgeChance
);
