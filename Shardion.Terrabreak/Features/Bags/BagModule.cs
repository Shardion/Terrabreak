using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;
using Serilog;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.Bags;

[SlashCommand("bag", "Grab bags of arbitrary text, which can be pulled from randomly.")]
public class BagModule(
    TerrabreakDatabaseContext db,
    IDbContextFactory<TerrabreakDatabaseContext> dbFactory,
    MenuManager menuManager) : TerrabreakApplicationCommandModule(menuManager)
{
    [SubSlashCommand("create", "Create a bag.")]
    public Task CreateBag(
        [SlashCommandParameter(Description = "The name of the bag.")]
        string name
    )
    {
        if (db.GetBag(Context.User.Id, name) is not null)
            return RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent($"Bag **`{name}`** already exists!")
                .WithFlags(MessageFlags.Ephemeral)
            ));
        db.CreateBag(Context.User.Id, name);
        db.SaveChanges();
        return RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            .WithContent($"Created bag **`{name}`.**")
        ));
    }

    [SubSlashCommand("add", "Add an entry to a bag.")]
    public Task AddBag(
        [SlashCommandParameter(Description = "The name of the bag.")]
        string name,
        [SlashCommandParameter(Description = "The entry to add to the bag.")]
        string entry
    )
    {
        if (db.GetBag(Context.User.Id, name) is not Bag bag)
            return RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent($"Bag **`{name}`** does not exist!")
                .WithFlags(MessageFlags.Ephemeral)
            ));

        db.AddToBag(bag, entry);
        db.SaveChanges();
        return RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            .WithContent($"Added to bag **`{name}`.**\n>>> {entry}")
        ));
    }

    [SubSlashCommand("take", "Remove a random entry from a bag.")]
    public Task TakeBag(
        [SlashCommandParameter(Description = "The name of the bag.")]
        string name
    )
    {
        if (db.GetBag(Context.User.Id, name) is not Bag bag)
            return RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent($"Bag **`{name}`** does not exist!")
                .WithFlags(MessageFlags.Ephemeral)
            ));

        BagEntry? entry = bag.Entries.Shuffle().FirstOrDefault();
        if (entry is null)
            return RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent($"Bag **`{name}`** is empty!")
                .WithFlags(MessageFlags.Ephemeral)
            ));

        return RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
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
