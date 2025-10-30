using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

public class CureBread : ICure
{
    public string Name => "Bread";
    public string Description => "A loaf of French bread. Use to cure all debuffs one time.";
    public Tier Tier => Tier.One;

    public int GetMaxUses(IPlayer wielder) => 1;
}
