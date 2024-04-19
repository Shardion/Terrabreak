using System;

namespace Shardion.Terrabreak.Features.Bags
{
    public class PendingEntry
    {
        public required string BagName { get; init; }
        public required string Entry { get; init; }
        public required DateTimeOffset WaitStartTime { get; init; }
    }
}
