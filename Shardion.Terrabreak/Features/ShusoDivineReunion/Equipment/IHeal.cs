using System.Diagnostics.Contracts;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

public interface IHeal : INamedEntity
{
    public Tier Tier { get; }

    [Pure]
    public int Heal(IPlayer wielder);
}
