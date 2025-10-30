using Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

public sealed record WeaponUnion : IWeapon
{
    public string Name => "Union Blade";
    public string Description => "A sword with a steel blade and a golden guard.";
    public Tier Tier => Tier.Two;
    public int DealDamage(IPlayer wielder, IEnemy target)
    {
        return 30;
    }
}

