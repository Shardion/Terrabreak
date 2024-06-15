using Discord;

namespace Shardion.Terrabreak.Services.Discord
{
    public class MessageBuilder
    {
        public string? Content { get; set; }
        public EmbedBuilder? Embed { get; set; }
        public ComponentBuilder? Components { get; set; }
        public AllowedMentions? AllowedMentions { get; set; }
        public MessageReference? Reference { get; set; }
        public MessageFlags? Flags { get; set; }

        public MessageBuilder WithContent(string? content)
        {
            Content = content;
            return this;
        }

        public MessageBuilder WithEmbed(EmbedBuilder? embed)
        {
            Embed = embed;
            return this;
        }

        public MessageBuilder WithComponents(ComponentBuilder? components)
        {
            Components = components;
            return this;
        }

        public MessageBuilder WithAllowedMentions(AllowedMentions? allowedMentions)
        {
            AllowedMentions = allowedMentions;
            return this;
        }

        public MessageBuilder WithAllowedMentions(ulong[]? users, ulong[]? roles)
        {
            AllowedMentions = new AllowedMentions()
            {
                UserIds = [.. users ?? []],
                RoleIds = [.. roles ?? []],
            };
            return this;
        }

        public MessageBuilder WithMessageFlags(MessageFlags? messageFlags)
        {
            Flags = messageFlags;
            return this;
        }

        public MessageBuilder Clone()
        {
            return new MessageBuilder()
            {
                Content = Content,
                Embed = Embed,
                Components = Components,
                AllowedMentions = AllowedMentions,
                Reference = Reference,
                Flags = Flags,
            };
        }

        public BuiltMessage Build()
        {
            return new BuiltMessage()
            {
                Content = Content ?? "",
                Embed = Embed?.Build(),
                Components = Components?.Build(),
                AllowedMentions = AllowedMentions,
                Reference = Reference,
                Flags = Flags ?? MessageFlags.None,
            };
        }
    }
}
