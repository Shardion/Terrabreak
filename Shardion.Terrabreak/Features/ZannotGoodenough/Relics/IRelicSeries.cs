namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics;

public interface IRelicSeries : INamedEntity
{
    public RelicTier Tier { get; }
    public string EmojiIdentifier { get; }
    public int StadiumPointsCost { get; }
}
