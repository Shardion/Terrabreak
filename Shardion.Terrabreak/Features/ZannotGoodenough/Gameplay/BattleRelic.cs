using Shardion.Terrabreak.Features.ZannotGoodenough.Relics;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;

public record BattleRelic(IRelic<RelicState> Relic)
{
    public RelicState State { get; } = Relic.NewState();
}
