using Discord;

namespace Shardion.Terrabreak.Services.Discord
{
    public class BuiltMessage
    {
        public required string Content { get; init; }
        public Embed? Embed { get; init; }
        public MessageComponent? Components { get; init; }
        public AllowedMentions? AllowedMentions { get; init; }
        public MessageReference? Reference { get; init; }
        public required MessageFlags Flags { get; init; }
    }
}
