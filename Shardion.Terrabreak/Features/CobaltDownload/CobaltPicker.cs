namespace Shardion.Terrabreak.Features.CobaltDownload;

public sealed class CobaltPicker
{
    public required string Type { get; init; }
    public required string Url { get; init; }
    public string? Thumb { get; init; }
}
