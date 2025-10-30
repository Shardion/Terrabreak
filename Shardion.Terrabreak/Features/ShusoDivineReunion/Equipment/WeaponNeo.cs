using Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

public sealed record WeaponNeo : IWeapon
{
    public string Name => "Neosaber";
    public string Description => "A new weapon with an edge of plasma.";
    public Tier Tier => Tier.Three;
    public int DealDamage(IPlayer wielder, IEnemy target)
    {
        return 45;
    }
}
