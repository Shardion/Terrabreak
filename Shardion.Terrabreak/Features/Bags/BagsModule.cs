using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Shardion.Terrabreak.Services.Database;

namespace Shardion.Terrabreak.Features.Bags
{
    [CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
    [IntegrationType(ApplicationIntegrationType.UserInstall, ApplicationIntegrationType.GuildInstall)]
    [Group("bag", "Bags, \"Watch Later\" lists for anything you want, which you can take random entries from.")]
    public class BagsModule : InteractionModuleBase
    {
        private readonly BagsFeature _bagsFeature;

        private readonly TerrabreakDatabaseContext _db;

        public BagsModule(TerrabreakDatabaseContext db, BagsFeature bagsFeature)
        {
            _db = db;
            _bagsFeature = bagsFeature;
        }

        [SlashCommand("create", "Creates a new bag.")]
        public async Task Create(
            [Summary(description: "The name of the bag to create.")] string name
        )
        {
            if (_db.GetBag(Context.User.Id, name) is not null)
            {
                await RespondAsync($"Bag **`{name}`** already exists!", ephemeral: true);
                return;
            }

            Bag bag = _db.CreateBag(Context.User.Id, name);
            await RespondAsync($"Bag **`{bag.Name}`** created.", ephemeral: true);
        }

        [SlashCommand("add", "Adds an entry to a bag.")]
        public async Task Add(
            [Summary(name: "bag", description: "The name of the bag to add the entry to.")] string bagName,
            [Summary(description: "The text of the entry.")] string entry
        )
        {
            Bag? bagSearchResult = _db.GetBag(Context.User.Id, bagName);
            if (bagSearchResult is not Bag bag)
            {
                await RespondAsync($"Bag **`{bagName}`** does not exist!", ephemeral: true);
                return;
            }

            if (!_db.AddToBag(bag, entry))
            {
                await RespondAsync($"Failed to add to bag **`{bag.Name}`**!\n> {entry}", ephemeral: true);
                return;
            }

            await RespondAsync($"Added to bag **`{bagName}`**.\n> {entry}");
        }

        [SlashCommand("take", "Displays a random entry from a bag, optionally removing it.")]
        public async Task Take(
            [Summary(name: "bag", description: "The name of the bag to take an entry from.")] string bagName
        )
        {
            Bag? bagSearchResult = _db.GetBag(Context.User.Id, bagName);
            if (bagSearchResult is not Bag bag)
            {
                await RespondAsync($"Bag **`{bagName}`** does not exist!", ephemeral: true);
                return;
            }

            if (bag.Entries.Count <= 0)
            {
                await RespondAsync($"Bag **`{bagName}`** is empty!", ephemeral: true);
                return;
            }

            string entryGuid = Guid.NewGuid().ToString();
            string stringifiedUserId = Context.User.Id.ToString(CultureInfo.InvariantCulture);
            string componentCustomId = $"remove:{stringifiedUserId},{entryGuid}";
            Emoji emoji = new("🗑️");

            string entry = bag.Entries[Random.Shared.Next(bag.Entries.Count)];

            _bagsFeature.PendingEntries[entryGuid] = new()
            {
                BagName = bag.Name,
                Entry = entry,
            };

            ComponentBuilder component = new ComponentBuilder()
                .WithButton("Remove from bag", customId: componentCustomId, emote: emoji, style: ButtonStyle.Danger);

            await RespondAsync($"Taken entry from bag **`{bagName}`**.\n> {entry}", components: component.Build());

            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(30));
                _bagsFeature.PendingEntries.TryRemove(entryGuid, out _);
                await ModifyOriginalResponseAsync((message) =>
                {
                    message.Components = new(new ComponentBuilder()
                        .WithButton("Remove from bag", emote: emoji, disabled: true, customId: "☃️", style: ButtonStyle.Danger).Build()
                    );
                });
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

            if (_bagsFeature.PendingEntries.TryRemove(entryGuid, out PendingEntry? nullableEntry) && nullableEntry is PendingEntry entry)
            {
                if (_db.GetBag(Context.User.Id, entry.BagName) is not Bag bag)
                {
                    await RespondAsync("Internal error while trying to get the bag associated with this entry. Please try again.", ephemeral: true);
                    return;
                }

                _db.RemoveFromBag(bag, entry.Entry);

                await RespondAsync($"Removed entry from bag **`{bag.Name}`**.", ephemeral: true);
            }
            else
            {
                await RespondAsync("Internal error while trying to get the entry associated with this message. Please try again.", ephemeral: true);
            }
        }

        [SlashCommand("delete", "Deletes an existing bag.")]
        public async Task Delete(
            [Summary(name: "bag", description: "The name of the bag to delete.")] string bagName,
            [Summary(description: "If bags which are not empty should be deleted.")] bool force = false
        )
        {
            Bag? bagSearchResult = _db.GetBag(Context.User.Id, bagName);
            if (bagSearchResult is not Bag bag)
            {
                await RespondAsync($"Bag **`{bagName}`** does not exist!", ephemeral: true);
                return;
            }

            if (bag.Entries.Count > 0 && !force)
            {
                await RespondAsync($"Bag **`{bag.Name}`** is not empty. Run this command again with `force:True` to delete **`{bag.Name}`**.", ephemeral: true);
                return;
            }

            if (!_db.DeleteBag(bag))
            {
                await RespondAsync($"Failed to delete bag **`{bag.Name}`**!", ephemeral: true);
                return;
            }

            await RespondAsync($"Bag **`{bag.Name}`** deleted.", ephemeral: true);
        }
    }
}
