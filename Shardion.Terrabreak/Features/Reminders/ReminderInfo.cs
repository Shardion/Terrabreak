using System;

namespace Shardion.Terrabreak.Features.Reminders
{
    internal class ReminderInfo
    {
        public required string Note { get; set; }
        public required DateTimeOffset StartTime { get; set; }
        public required string UserId { get; set; }
        public string? ChannelId { get; set; }
    }
}
