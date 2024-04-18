using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace Shardion.Terrabreak.Features.SelfModeration
{
    [CommandContextType(InteractionContextType.Guild)]
    [IntegrationType(ApplicationIntegrationType.GuildInstall)]
    public class SelfModerationModule : InteractionModuleBase
    {
        [SlashCommand("selfpurge", "Deletes all of your messages within a specified timeframe.")]
        public async Task SelfPurge(int minutes)
        {
            if (Context.Channel is not ITextChannel textChannel)
            {
                await Context.Interaction.RespondAsync("Cannot purge messages in non-text channel.\nP.S. Please tell a developer how you ran a slash command in a non-text channel!", ephemeral: true);
                return;
            }

            IAsyncEnumerable<IReadOnlyCollection<IMessage>> messages = Context.Channel.GetMessagesAsync(1000, CacheMode.AllowDownload);
            ConcurrentBag<IMessage> messagesToDelete = [];

            if (minutes <= 0)
            {
                await Context.Interaction.RespondAsync("Invalid time.", ephemeral: true);
            }

            await Context.Interaction.DeferAsync(ephemeral: true);
            await Parallel.ForEachAsync(messages, async (messageChunk, ct) =>
            {
                await Parallel.ForEachAsync(messageChunk.Where((message) => message.Author.Id == Context.User.Id && (DateTime.UtcNow - message.CreatedAt).TotalMinutes <= minutes), (targetedMessage, innerCt) =>
                {
                    messagesToDelete.Add(targetedMessage);
                    return ValueTask.CompletedTask;
                });
            });

            await textChannel.DeleteMessagesAsync(messagesToDelete);
            await Context.Interaction.FollowupAsync($"{messagesToDelete.Count} messages deleted.");
        }
    }
}
