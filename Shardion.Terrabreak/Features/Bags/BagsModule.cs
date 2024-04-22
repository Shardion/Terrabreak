using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog;
using Shardion.Terrabreak.Services.Discord;

namespace Shardion.Terrabreak.Features.Bags
{
    [CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
    [IntegrationType(ApplicationIntegrationType.UserInstall, ApplicationIntegrationType.GuildInstall)]
    [Group("bag", "Bags, \"Watch Later\" lists for anything you want, which you can take random entries from.")]
    public class BagsModule : InteractionModuleBase
    {
        private static readonly ConcurrentDictionary<string, PendingEntry> _pendingEntries = [];
        private readonly BagCollectionManager _bags;

        public BagsModule(BagCollectionManager bags)
        {
            _bags = bags;
        }

        [SlashCommand("create", "Creates a new bag.")]
        public async Task Create(
            [Summary(description: "The name of the bag to create")] string name
        )
        {
            if (_bags.GetBag(Context.User.Id, name) is not null)
            {
                await RespondAsync($"Bag **`{name}`** already exists!", ephemeral: true);
                return;
            }

            Bag bag = _bags.CreateBag(Context.User.Id, name);
            await RespondAsync($"Bag **`{bag.Name}`** created.", ephemeral: true);
        }

        [SlashCommand("add", "Adds an entry to a bag.")]
        public async Task Add(
            [Summary(description: "The name of the bag to add the entry to")] string name,
            [Summary(description: "The text of the entry")] string entry
        )
        {
            Bag? bagSearchResult = _bags.GetBag(Context.User.Id, name);
            if (bagSearchResult is not Bag bag)
            {
                await RespondAsync($"Bag **`{name}`** does not exist!", ephemeral: true);
                return;
            }

            if (!_bags.AddToBag(bag, entry))
            {
                await RespondAsync($"Failed to add to bag **`{bag.Name}`**!\n> {entry}", ephemeral: true);
                return;
            }

            await RespondAsync($"Added to bag **`{name}`**.\n> {entry}");
        }

        [SlashCommand("take", "Displays a random entry from a bag, optionally removing it.")]
        public async Task Take(
            [Summary(description: "The name of the bag to take an entry from")] string name
        )
        {
            Bag? bagSearchResult = _bags.GetBag(Context.User.Id, name);
            if (bagSearchResult is not Bag bag)
            {
                await RespondAsync($"Bag **`{name}`** does not exist!", ephemeral: true);
                return;
            }

            if (bag.Entries.Count <= 0)
            {
                await RespondAsync($"Bag **`{name}`** is empty!", ephemeral: true);
                return;
            }

            string entryGuid = Guid.NewGuid().ToString();
            string stringifiedUserId = Context.User.Id.ToString(CultureInfo.InvariantCulture);
            string componentCustomId = $"remove:{stringifiedUserId},{entryGuid}";
            Emoji emoji = new("🗑️");

            string entry = bag.Entries[Random.Shared.Next(bag.Entries.Count)];

            _pendingEntries[entryGuid] = new()
            {
                BagName = bag.Name,
                Entry = entry,
            };

            var component = new ComponentBuilder()
                .WithButton("Remove from bag", customId: componentCustomId, emote: emoji);

            await RespondAsync($"Taken entry from bag **`{name}`**.\n> {entry}", components: component.Build());

            await Task.Delay(TimeSpan.FromSeconds(30));
            _pendingEntries.TryRemove(entryGuid, out _);
            await ModifyOriginalResponseAsync((message) =>
            {
                message.Components = new(new ComponentBuilder()
                    .WithButton("Remove from bag", emote: emoji, disabled: true, customId: "☃️").Build()
                );
            });
        }

        [ComponentInteraction("remove:*,*", true)]
        public async Task Remove(ulong intendedUserId, string entryGuid)
        {
            if (Context.User.Id != intendedUserId)
            {
                await RespondAsync("You are not worthy.", ephemeral: true);
                return;
            }

            if (_pendingEntries.TryRemove(entryGuid, out PendingEntry? nullableEntry) && nullableEntry is PendingEntry entry)
            {
                if (_bags.GetBag(Context.User.Id, entry.BagName) is not Bag bag)
                {
                    await RespondAsync("Internal error while trying to get the bag associated with this entry. Please try again.", ephemeral: true);
                    return;
                }

                _bags.RemoveFromBag(bag, entry.Entry);

                await RespondAsync($"Removed entry from bag **`{bag.Name}`**.", ephemeral: true);
            }
            else
            {
                await RespondAsync("Internal error while trying to get the entry associated with this message. Please try again.", ephemeral: true);
            }
        }

        [SlashCommand("delete", "Deletes an existing bag")]
        public async Task Delete(
            [Summary(description: "The name of the bag to delete")] string name,
            [Summary(description: "If bags which are not empty should be deleted")] bool force = false
        )
        {
            Bag? bagSearchResult = _bags.GetBag(Context.User.Id, name);
            if (bagSearchResult is not Bag bag)
            {
                await RespondAsync($"Bag **`{name}`** does not exist!", ephemeral: true);
                return;
            }

            if (bag.Entries.Count > 0 && !force)
            {
                await RespondAsync($"Bag **`{bag.Name}`** is not empty. Run this command again with `force:True` to delete **`{bag.Name}`**.", ephemeral: true);
                return;
            }

            if (!_bags.DeleteBag(bag))
            {
                await RespondAsync($"Failed to delete bag **`{bag.Name}`**!", ephemeral: true);
                return;
            }

            await RespondAsync($"Bag **`{bag.Name}`** deleted.", ephemeral: true);
        }
    }
}
