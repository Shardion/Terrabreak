using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Services.Discord
{
    public sealed class DiscordManagerOptions : IStaticOptions
    {
        public string SectionName => "Discord";

        public required string Token { get; set; }
    }
}
