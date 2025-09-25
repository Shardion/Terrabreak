using NetCord;
using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Services.Identity;

public sealed class IdentityOptions : IDynamicOptions
{
    public string SectionName => "Identity";

    public OptionsPermissions Permissions => new()
    {
        Bot = OptionsAccessibility.ReadWrite,
        Servers = OptionsAccessibility.None,
        Users = OptionsAccessibility.None
    };

    public string BotName { get; set; } = "Project Terrabreak";
    public Color BotColor { get; set; } = new(185, 236, 184);
    public string[]? Splashes { get; set; } = null;
    public string[]? AccessDeniedResponses { get; set; } = null;

    public ulong? PrimaryInstanceOwnerId { get; set; } = null;
    public ulong? PrimaryDevelopmentServerId { get; set; } = null;
    public ulong[] InstanceOwnerIds { get; set; } = [];
    public ulong[] DevelopmentServerIds { get; set; } = [];
    public string? CurrentVersion { get; set; }
}
