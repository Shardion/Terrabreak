using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.Bags;

[SlashCommand("bag", "Grab bags of arbitrary text, which can be pulled from randomly.",
    Contexts = [InteractionContextType.BotDMChannel, InteractionContextType.DMChannel, InteractionContextType.Guild])]
public class BagModule(
    TerrabreakDatabaseContext db,
    IDbContextFactory<TerrabreakDatabaseContext> dbFactory,
    MenuManager menuManager) : TerrabreakApplicationCommandModule(menuManager)
{
    [SubSlashCommand("create", "Create a bag.")]
    public async Task CreateBag(
        [SlashCommandParameter(Description = "The name of the bag.")]
        string name
    )
    {
        if (db.GetBag(Context.User.Id, name) is not null)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent($"Bag **`{name}`** already exists!")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        db.CreateBag(Context.User.Id, name);
        await Task.WhenAll(
            db.SaveChangesAsync(),
            RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent($"Created bag **`{name}`.**")
        )));
    }

    [SubSlashCommand("add", "Add an entry to a bag.")]
    public async Task AddBag(
        [SlashCommandParameter(Description = "The name of the bag.")]
        string name,
        [SlashCommandParameter(Description = "The entry to add to the bag.")]
        string entry
    )
    {
        if (db.GetBag(Context.User.Id, name) is not Bag bag)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent($"Bag **`{name}`** does not exist!")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        db.AddToBag(bag, entry);
        await Task.WhenAll(
            db.SaveChangesAsync(),
            RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent($"Added to bag **`{name}`.**\n>>> {entry}")
        )));
    }

    [SubSlashCommand("take", "Remove a random entry from a bag.")]
    public async Task TakeBag(
        [SlashCommandParameter(Description = "The name of the bag.")]
        string name
    )
    {
        if (db.GetBag(Context.User.Id, name) is not Bag bag)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent($"Bag **`{name}`** does not exist!")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        BagEntry? entry = bag.Entries.Shuffle().FirstOrDefault();
        if (entry is null)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent($"Bag **`{name}`** is empty!")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            .WithContent($"Taken entry from bag **`{name}`.**\n>>> {entry.Text}")
            .WithComponents([
                new ActionRowProperties().AddComponents(new ButtonProperties($"bag-remove:{entry.Id.ToString()}",
                    "Remove", ButtonStyle.Primary))
            ])));
    }

    [SubSlashCommand("view", "List and delete the contents of the bag.")]
    public async Task ViewBag(
        [SlashCommandParameter(Description = "The name of the bag.")]
        string name
    )
    {
        // Existence check is done in BagViewMenu
        await ActivateMenuAsync(new BagViewMenu(dbFactory, name)
        {
            AllowedUsers = new HashSet<ulong>([Context.Interaction.User.Id])
        });
    }
}
