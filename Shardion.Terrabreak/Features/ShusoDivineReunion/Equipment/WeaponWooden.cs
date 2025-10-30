using Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

public sealed record WeaponWooden : IWeapon
{
    public string Name => "Wooden Blade";
    public string Description => "It's actually just a stick, but don't tell the game designers...";
    public Tier Tier => Tier.One;

    public int DealDamage(IPlayer wielder, IEnemy target)
    {
        return 15;
    }
}
