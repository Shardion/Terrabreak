using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

public sealed record ShieldHoly : IShield
{
    public string Name => "Holy Shield";
    public string Description => "A purple knight's shield with a golden symbol.";
    public Tier Tier => Tier.Three;

    public int IncreaseMaxHealth(int currentMaxHealth, IPlayer wielder)
    {
        return currentMaxHealth + 200;
    }
}
