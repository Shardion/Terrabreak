using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        public async Task SelfPurge(
            [Summary(description: "How many minutes of messages to delete.")] int minutes
        )
        {
            if (Context.Channel is not ITextChannel textChannel)
            {
                await RespondAsync("Cannot purge messages in non-text channel.\nP.S. Please tell a developer how you ran a slash command in a non-text channel!", ephemeral: true);
                return;
            }

            IAsyncEnumerable<IReadOnlyCollection<IMessage>> messages = Context.Channel.GetMessagesAsync(1000, CacheMode.AllowDownload);
            ConcurrentBag<IMessage> messagesToDelete = [];

            if (minutes <= 0)
            {
                await RespondAsync("Invalid time.", ephemeral: true);
                return;
            }

            await DeferAsync(ephemeral: true);
            await Parallel.ForEachAsync(messages, async (messageChunk, ct) =>
            {
                await Parallel.ForEachAsync(messageChunk.Where((message) => message.Author.Id == Context.User.Id && (DateTime.UtcNow - message.CreatedAt).TotalMinutes <= minutes), (targetedMessage, innerCt) =>
                {
                    messagesToDelete.Add(targetedMessage);
                    return ValueTask.CompletedTask;
                });
            });

            await textChannel.DeleteMessagesAsync(messagesToDelete);
            await FollowupAsync($"{messagesToDelete.Count} messages deleted.");
        }
    }
}
