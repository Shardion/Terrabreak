using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion;

public sealed class Registry<TThing> where TThing : INamedEntity
{
    public FrozenDictionary<string, TThing> Forward { get; }
    public FrozenSet<TThing> Contents { get; }

    public Registry(IReadOnlyCollection<TThing> things)
    {
        Forward = things.ToFrozenDictionary<TThing, string, TThing>(thing => thing.InternalName, thing => thing);
        Contents = things.ToFrozenSet();
    }
}
