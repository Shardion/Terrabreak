namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.InvestorCore;

public class InvestorCore : IRelicSeries
{
    public string Name => "Investor Core";
    public string Description =>
        "The next-generation version of the CORE series is here! Improved usability, extra power, CORE is the first choice. Choose wisely, choose Investor!";
    public string EmojiIdentifier => "investorcore";
    public RelicTier Tier => RelicTier.TierOne;
    public int StadiumPointsCost => 100;
}
