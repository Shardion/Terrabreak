using System.Collections.Concurrent;

namespace Shardion.Terrabreak.Features.Automute
{
    public class MessageAutomuteStatus
    {
        public ulong Id { get; set; }
        public ConcurrentBag<ulong> MuteReactors { get; set; } = [];
        public bool MuteTriggered { get; set; }
    }
}
