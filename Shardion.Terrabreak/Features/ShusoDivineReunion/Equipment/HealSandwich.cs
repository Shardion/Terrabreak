using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

public sealed record HealSandwich : IHeal
{
    public string Name => "Sandwich";
    public string Description => "Favored by many due to its low cost. Use to recover 50 HP.";
    public Tier Tier => Tier.One;

    public int Heal(IPlayer wielder)
    {
        return 50;
    }
}
