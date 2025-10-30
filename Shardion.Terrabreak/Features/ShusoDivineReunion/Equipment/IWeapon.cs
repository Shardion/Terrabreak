using System.Diagnostics.Contracts;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

public interface IWeapon : INamedEntity
{
    public Tier Tier { get; }

    [Pure]
    public int DealDamage(IPlayer wielder, IEnemy target);
}
