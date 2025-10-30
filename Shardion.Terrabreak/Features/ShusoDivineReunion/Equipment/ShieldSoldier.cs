using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

public sealed record ShieldSoldier : IShield
{
    public string Name => "Soldier Shield";
    public string Description => "A broad, triangular shield, made of steel.";
    public Tier Tier => Tier.Two;

    public int IncreaseMaxHealth(int currentMaxHealth, IPlayer wielder)
    {
        return currentMaxHealth + 100;
    }
}
