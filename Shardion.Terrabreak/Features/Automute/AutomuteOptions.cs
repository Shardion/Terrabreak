using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Features.Automute
{
    public sealed class AutomuteOptions : IDynamicOptions
    {
        public string SectionName => "Automute";
        public OptionsPermissions Permissions => new()
        {
            Bot = OptionsAccessibility.ReadWrite,
            Servers = OptionsAccessibility.ReadWrite,
            Users = OptionsAccessibility.None,
        };

        public bool Enabled { get; set; } = false;
        public uint ReactionsRequired { get; set; } = 3;
        public string Emoji { get; set; } = "🐴"; // horse
    }
}
