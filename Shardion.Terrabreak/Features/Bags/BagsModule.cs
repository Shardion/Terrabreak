using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace Shardion.Terrabreak.Features.Bags
{
    [CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
    [IntegrationType(ApplicationIntegrationType.UserInstall, ApplicationIntegrationType.GuildInstall)]
    [Group("bag", "Bags, \"Watch Later\" lists for anything you want, which you can randomly pull from")]
    public class BagsModule : InteractionModuleBase
    {
        private readonly BagCollectionManager _bags;

        public BagsModule(BagCollectionManager bags)
        {
            _bags = bags;
        }

        [SlashCommand("create", "Creates a new bag")]
        public async Task Create(
            [Summary(description: "The name of the bag to create")] string name
        )
        {
            if (_bags.GetBag(Context.User.Id, name) is Bag existingBag)
            {
                await Context.Interaction.RespondAsync($"Bag **`{name}`** already exists!", ephemeral: true);
                return;
            }

            Bag bag = _bags.CreateBag(Context.User.Id, name);
            await Context.Interaction.RespondAsync($"Bag **`{bag.Name}`** created.", ephemeral: true);
        }

        [SlashCommand("add", "Adds an entry to a bag")]
        public async Task Add(
            [Summary(description: "The name of the bag to add the entry to")] string name,
            [Summary(description: "The text of the entry")] string entry
        )
        {
            Bag? bagSearchResult = _bags.GetBag(Context.User.Id, name);
            if (bagSearchResult is not Bag bag)
            {
                await Context.Interaction.RespondAsync($"Bag **`{name}`** does not exist!", ephemeral: true);
                return;
            }

            if (!_bags.AddToBag(bag, entry))
            {
                await Context.Interaction.RespondAsync($"Failed to add to bag **`{bag.Name}`**!\n> {entry}", ephemeral: true);
                return;
            }

            await Context.Interaction.RespondAsync($"Added to bag **`{name}`**.\n> {entry}");
        }

        [SlashCommand("take", "Displays and removes a random entry from a bag")]
        public async Task Take(
            [Summary(description: "The name of the bag to take an entry from")] string name
        )
        {
            Bag? bagSearchResult = _bags.GetBag(Context.User.Id, name);
            if (bagSearchResult is not Bag bag)
            {
                await Context.Interaction.RespondAsync($"Bag **`{name}`** does not exist!", ephemeral: true);
                return;
            }

            string entry = bag.Entries[(Random.Shared.Next(bag.Entries.Count))];
            _bags.RemoveFromBag(bag, entry);

            await Context.Interaction.RespondAsync($"Taken entry from bag **`{name}`**.\n> {entry}");
        }

        [SlashCommand("remove", "Removes an entry from the bag")]
        public async Task Remove(
            [Summary(description: "The name of the bag to remove an entry from")] string name
        )
        {
            Bag? bagSearchResult = _bags.GetBag(Context.User.Id, name);
            if (bagSearchResult is not Bag bag)
            {
                await Context.Interaction.RespondAsync($"Bag **`{name}`** does not exist!", ephemeral: true);
                return;
            }

            string entry = bag.Entries[(Random.Shared.Next(bag.Entries.Count))];

            await Context.Interaction.RespondAsync($"Taken entry from bag **`{name}`**.\n> {entry}");
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
                await Context.Interaction.RespondAsync($"Bag **`{name}`** does not exist!", ephemeral: true);
                return;
            }

            if (bag.Entries.Count > 0 && !force)
            {
                await Context.Interaction.RespondAsync($"Bag **`{bag.Name}`** is not empty. Run this command again with `force:True` to delete **`{bag.Name}`**.", ephemeral: true);
                return;
            }

            if (!_bags.DeleteBag(bag))
            {
                await Context.Interaction.RespondAsync($"Failed to delete bag **`{bag.Name}`**!", ephemeral: true);
                return;
            }

            await Context.Interaction.RespondAsync($"Bag **`{bag.Name}`** deleted.", ephemeral: true);
        }
    }
}
