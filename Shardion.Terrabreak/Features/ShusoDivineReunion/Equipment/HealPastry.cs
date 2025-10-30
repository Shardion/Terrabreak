using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

public sealed record HealPastry : IHeal
{
    public string Name => "Real Pastry";

    public string Description =>
        "A large cookie in the shape of a face, a local specialty that's sold millions. Use to recover an incredible amount of HP.";
    public Tier Tier => Tier.Four;

    public int Heal(IPlayer wielder)
    {
        return 200;
    }
}
