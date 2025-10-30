using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

public class CureTeraBread : ICure
{
    public string Name => "Tera Bread";
    public string Description => "A very, very large bread, just for you! Use to cure all debuffs four times.";
    public Tier Tier => Tier.Four;

    public int GetMaxUses(IPlayer wielder) => 4;
}
