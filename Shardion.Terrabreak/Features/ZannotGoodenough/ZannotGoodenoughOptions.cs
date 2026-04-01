using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Features.ZannotGoodenough;

public sealed class ZannotGoodenoughOptions : IDynamicOptions
{
    public string SectionName => "ZannotGoodenough";
    public OptionsPermissions Permissions => new()
    {
        Bot = OptionsAccessibility.ReadWrite,
        Servers = OptionsAccessibility.None,
        Users = OptionsAccessibility.None,
    };

    public ulong? PresidentZannotUserId { get; set; } = null;
}
