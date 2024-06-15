using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;

namespace Shardion.Terrabreak.Services.Discord
{
    public static class IMessageChannelBuiltMessageExtensions
    {
        public static Task<IUserMessage> SendMessageAsync(this IMessageChannel channel, BuiltMessage message, RequestOptions? options = null)
        {
            return channel.SendMessageAsync(
                text: message.Content,
                isTTS: false,
                embed: message.Embed,
                options: options,
                allowedMentions: message.AllowedMentions,
                messageReference: message.Reference,
                components: message.Components,
                stickers: null,
                embeds: null,
                flags: message.Flags
            );
        }

        public static Task<IUserMessage> SendFileAsync(this IMessageChannel channel, string filePath, BuiltMessage message, RequestOptions? options = null)
        {
            return channel.SendFileAsync(
                filePath,
                text: message.Content,
                isTTS: false,
                embed: message.Embed,
                options: options,
                allowedMentions: message.AllowedMentions,
                messageReference: message.Reference,
                components: message.Components,
                stickers: null,
                embeds: null,
                flags: message.Flags
            );
        }

        public static Task<IUserMessage> SendFilesAsync(this IMessageChannel channel, IEnumerable<FileAttachment> attachments, BuiltMessage message, RequestOptions? options = null)
        {
            return channel.SendFilesAsync(
                attachments,
                text: message.Content,
                isTTS: false,
                embed: message.Embed,
                options: options,
                allowedMentions: message.AllowedMentions,
                messageReference: message.Reference,
                components: message.Components,
                stickers: null,
                embeds: null,
                flags: message.Flags
            );
        }

        public static Task<IUserMessage> ModifyMessageAsync(this IMessageChannel channel, ulong messageId, BuiltMessage message, RequestOptions? options = null)
        {
            return channel.ModifyMessageAsync(messageId, (properties) =>
            {
                properties.Content = message.Content;
                properties.Components = message.Components;
                properties.Embed = message.Embed;
                properties.Flags = message.Flags;
                properties.AllowedMentions = message.AllowedMentions;
            }, options: options);
        }
    }
}
