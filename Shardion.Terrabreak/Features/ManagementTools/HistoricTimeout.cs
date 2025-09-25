using System;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Unicode;
using Serilog;

namespace Shardion.Terrabreak.Features.ManagementTools;

public sealed class HistoricTimeout
{
    public required Guid Id { get; init; }
    public required string Identifier { get; init; }
    public required byte[] Data { get; init; }
    public required DateTimeOffset ExpirationDate { get; init; }
    public required bool ExpiryProcessed { get; init; }
    public required bool ShouldRetry { get; init; }

    public HistoricReminderInfo? GetDeserializedData()
    {
        return JsonSerializer.Deserialize<HistoricReminderInfo>(Data);
    }
}
