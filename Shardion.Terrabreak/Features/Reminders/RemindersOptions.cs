using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Features.Reminders
{
    public sealed class RemindersOptions : IDynamicOptions
    {
        public string SectionName => "Reminders";

        public ulong? FallbackChannelId { get; set; }

        public OptionsPermissions Permissions => new()
        {
            Bot = OptionsAccessibility.ReadWrite,
            Servers = OptionsAccessibility.ReadWrite,
            Users = OptionsAccessibility.None,
        };
    }
}
