using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

public class CureGigaBread : ICure
{
    public string Name => "Giga Bread";
    public string Description => "A comically large loaf of French bread. Use to cure all debuffs three times.";
    public Tier Tier => Tier.Three;

    public int GetMaxUses(IPlayer wielder) => 3;
}
