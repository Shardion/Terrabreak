using System;
using System.Collections.Generic;
using LiteDB;

namespace Shardion.Terrabreak.Features.Bags
{
    public class Bag
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();

        public required string Name { get; set; }
        public required ulong? OwnerId { get; set; }
        public required List<string> Entries { get; set; }
    }
}
