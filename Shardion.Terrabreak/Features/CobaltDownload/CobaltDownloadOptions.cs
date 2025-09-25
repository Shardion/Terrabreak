using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Features.CobaltDownload;

public sealed class CobaltDownloadOptions : IDynamicOptions
{
    public string SectionName => "CobaltDownload";

    public OptionsPermissions Permissions => new()
    {
        Bot = OptionsAccessibility.ReadWrite,
        Servers = OptionsAccessibility.None,
        Users = OptionsAccessibility.None
    };

    public string? CobaltAPIUrl { get; set; }
    public string? CobaltAPIKey { get; set; }
}
