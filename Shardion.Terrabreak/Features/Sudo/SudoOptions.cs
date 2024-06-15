using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Features.Sudo
{
    public sealed class SudoOptions : IDynamicOptions
    {
        public string SectionName => "Sudo";
        public OptionsPermissions Permissions => new()
        {
            Bot = OptionsAccessibility.None,
            Servers = OptionsAccessibility.ReadWrite,
            Users = OptionsAccessibility.None,
        };

        public ulong[] SudoerUserIDs { get; set; } = [];
        public ulong? SudoRoleId { get; set; }
    }
}
