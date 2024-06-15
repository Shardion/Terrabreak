using System;

namespace Shardion.Terrabreak.Features.Reminders
{
    public class ReminderListState
    {
        public bool Delayed { get; set; } = false;
        public int Page { get; set; } = 0;
        public Guid?[] ReminderGuids { get; set; } = [];
    }
}
