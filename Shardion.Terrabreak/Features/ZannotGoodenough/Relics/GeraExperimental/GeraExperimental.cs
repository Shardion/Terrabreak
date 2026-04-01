using System.Collections.Generic;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.GeraExperimental;

public class GeraExperimental : IRelicSeries
{
    public string Name => "Gera Experimental";
    public string Description => "\"So we had Gera throw together a prototype.\"";
    public string EmojiIdentifier => "geraexperimental";
    public RelicTier Tier => RelicTier.TierThree;
    public int StadiumPointsCost => 500;
}
