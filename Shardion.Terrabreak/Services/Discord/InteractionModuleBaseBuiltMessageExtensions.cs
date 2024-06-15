using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace Shardion.Terrabreak.Services.Discord
{
    public class TerrabreakInteractionModuleBase : InteractionModuleBase
    {
        public Task RespondAsync(BuiltMessage message, RequestOptions? options = null)
        {
            return RespondAsync(
                text: message.Content,
                isTTS: false,
                embed: message.Embed,
                options: options,
                allowedMentions: message.AllowedMentions,
                components: message.Components,
                embeds: null
            );
        }

        public Task<IUserMessage> FollowupAsync(BuiltMessage message, RequestOptions? options = null)
        {
            return FollowupAsync(
                text: message.Content,
                isTTS: false,
                embed: message.Embed,
                options: options,
                allowedMentions: message.AllowedMentions,
                components: message.Components,
                embeds: null
            );
        }

        public Task RespondWithFileAsync(string filePath, BuiltMessage message, RequestOptions? options = null)
        {
            return RespondWithFileAsync(
                filePath,
                text: message.Content,
                isTTS: false,
                embed: message.Embed,
                options: options,
                allowedMentions: message.AllowedMentions,
                components: message.Components,
                embeds: null
            );
        }
    }
}
