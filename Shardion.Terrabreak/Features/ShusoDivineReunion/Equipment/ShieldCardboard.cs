using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

public sealed record ShieldCardboard : IShield
{
    public string Name => "Cardboard Shield";
    public string Description => "Cuts down on manufacturing costs.";
    public Tier Tier => Tier.One;

    public int IncreaseMaxHealth(int currentMaxHealth, IPlayer wielder)
    {
        return currentMaxHealth + 0;
    }
}
