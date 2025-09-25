using System;
using System.Collections.Generic;

namespace Shardion.Terrabreak.Features.ManagementTools;

public sealed class HistoricBag
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    // In 3.1, this field was nullable, but I don't recall any code supporting a null OwnerId, and the 3.1 source
    // type-checks successfully even if I remove the nullability, so I'm just going to make this non-nullable...
    public required ulong OwnerId { get; set; }
    public required List<string> Entries { get; set; }
}
