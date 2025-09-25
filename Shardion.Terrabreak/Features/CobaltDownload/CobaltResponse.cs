using System.Collections.Generic;

namespace Shardion.Terrabreak.Features.CobaltDownload;

public sealed class CobaltResponse
{
    public required string Status { get; init; }

    public string? Url { get; init; }
    public string? Filename { get; init; }

    public string? Audio { get; init; }
    public string? AudioFilename { get; init; }
    public IReadOnlyCollection<CobaltPicker>? Picker { get; init; }

    public CobaltError? Error { get; init; }
}
