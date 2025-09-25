using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shardion.Terrabreak.Features.Bags;

public class Bag
{
    [Key] public Guid Id { get; init; }
    public required string Name { get; init; }
    public required ulong? OwnerId { get; init; }

    public ICollection<BagEntry> Entries { get; init; } = [];
}
