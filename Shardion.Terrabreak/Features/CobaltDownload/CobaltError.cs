namespace Shardion.Terrabreak.Features.CobaltDownload;

public sealed class CobaltError
{
    public required string Code { get; init; }
    public CobaltErrorContext? Context { get; init; }
}
