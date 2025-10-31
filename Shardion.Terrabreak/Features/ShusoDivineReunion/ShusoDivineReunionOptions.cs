using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion;

public sealed class ShusoDivineReunionOptions : IDynamicOptions
{
    public string SectionName => "ShusoDivineReunion";

    public OptionsPermissions Permissions => new()
    {
        Bot = OptionsAccessibility.ReadWrite,
        Servers = OptionsAccessibility.None,
        Users = OptionsAccessibility.None
    };

    public ulong? TakeoverServerId { get; set; } = null;
    public bool InhibitTakeoverMessage { get; set; } = false;
}
