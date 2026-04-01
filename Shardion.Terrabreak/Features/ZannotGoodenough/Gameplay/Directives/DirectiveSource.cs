namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;

public record DirectiveSource(
    object Cause,
    BattleRelic? Relic,
    BattleMonster? Monster
);
