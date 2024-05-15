using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shardion.Terrabreak.Features.Bags
{
    public class Bag
    {
        [Key]
        public Guid Id { get; set; }

        public required string Name { get; set; }
        public required ulong? OwnerId { get; set; }
        public required List<string> Entries { get; set; }
    }
}
