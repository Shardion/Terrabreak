namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.PresidentZannotFinest;

public class PresidentZannotFinest : IRelicSeries
{
    public string Name => "President Zannot's Finest";
    public string InternalName => "PresidentZannotFinest";
    public string Description =>
        "Approved by the President himself, hand-crafted by Luminous artisans, this exquisite set of charms will guide you straight to his success!";
    public string EmojiIdentifier => "presidentzannotfinest";
    public RelicTier Tier => RelicTier.TierTwo;
    public int StadiumPointsCost => 250;
}
