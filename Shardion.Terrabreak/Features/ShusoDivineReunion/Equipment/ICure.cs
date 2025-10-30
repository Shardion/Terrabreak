using System.Diagnostics.Contracts;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;

public interface ICure : INamedEntity
{
    public Tier Tier { get; }

    [Pure]
    public int GetMaxUses(IPlayer wielder);
}

