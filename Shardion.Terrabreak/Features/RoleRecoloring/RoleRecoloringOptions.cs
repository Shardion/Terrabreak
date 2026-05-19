using System.Collections.Generic;
using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Features.RoleRecoloring;

public sealed class RoleRecoloringOptions : IDynamicOptions
{
    public string SectionName => "RoleRecoloring";
    public OptionsPermissions Permissions => new()
    {
        Bot = OptionsAccessibility.ReadWrite,
        Servers = OptionsAccessibility.None,
        Users = OptionsAccessibility.None
    };

    // TODO: Doesn't allow configuration per-server. Oh well...?
    public Dictionary<ulong, ulong> UserRecolorableRoles { get; } = [];
}
