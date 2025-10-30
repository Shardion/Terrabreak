using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

public sealed record ShieldMantle : IShield
{
    public string Name => "Mantle Mirror";
    public string Description => "A large, hexagonal device, capable of reflecting incredible amounts of energy.";
    public Tier Tier => Tier.Four;

    public int IncreaseMaxHealth(int currentMaxHealth, IPlayer wielder)
    {
        return currentMaxHealth + 300;
    }
}
