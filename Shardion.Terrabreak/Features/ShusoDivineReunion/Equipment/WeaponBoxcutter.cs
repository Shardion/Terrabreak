using Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

public sealed record WeaponBoxcutter : IWeapon
{
    public string Name => "Boxcutter";

    public string Description =>
        "An experimental polearm-type implement, with a pink, glass-like edge.";
    public Tier Tier => Tier.Four;

    public int DealDamage(IPlayer wielder, IEnemy target)
    {
        return 60;
    }
}
