namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.TricksOfTheTrade;

public class TricksOfTheTrade : IRelicSeries
{
    public string Name => "Tricks of the Trade";
    public string InternalName => "TricksOfTheTrade";
    public string Description => "\"Good news, President! We've secured a deal with...\"";
    public string EmojiIdentifier => "tricksofthetrade";
    public RelicTier Tier => RelicTier.TierFour;
    public int StadiumPointsCost => 1500;
}
