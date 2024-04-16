using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Services.Identity
{
    public sealed class IdentityOptions : IDynamicOptions
    {
        public string SectionName => "Identity";
        public OptionsPermissions Permissions => new()
        {
            Bot = OptionsAccessibility.ReadWrite,
            Servers = OptionsAccessibility.None,
            Users = OptionsAccessibility.None,
        };

        public string BotName { get; set; } = "Project Terrabreak";
        public uint BotColor { get; set; } = 0x1f1e33;

        public string[] Splashes { get; set; } =
        [
            "Evolving, endlessly"
        ];

        public ulong? PrimaryDeveloperID { get; set; } = null;
        public ulong? PrimaryDevelopmentServerID { get; set; } = null;
        public ulong[] DeveloperIDs { get; set; } = [];
        public ulong[] DevelopmentServerIDs { get; set; } = [];
    }
}
