using System;

namespace Shardion.Terrabreak.Features.ManagementTools;

public sealed class HistoricReminderInfo
{
    public required string Note { get; init; }
    public required DateTimeOffset StartTime { get; init; }
    public required string UserId { get; init; }
    public string? ChannelId { get; init; }
}
