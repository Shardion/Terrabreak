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
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.Bags;

public class BagViewMenu(IDbContextFactory<TerrabreakDatabaseContext> dbFactory, string name) : TerrabreakMenu
{
    public Bag? TargetBag { get; set; } = null;
    public int PageEntryCount { get; set; } = 10;
    public int PageNumber { get; set; } = 0;

    public override Task<MenuMessage> BuildMessage()
    {
        if (TargetBag is null)
            throw new InvalidOperationException("Cannot build a bag view message without a bag to build from!");

        // Integer division rounds towards zero
        int fullPages = TargetBag.Entries.Count / PageEntryCount;
        // Add page for remainder entries
        bool remainderPage = TargetBag.Entries.Count % PageEntryCount > 0;
        int totalPages = remainderPage ? fullPages + 1 : fullPages;

        if (PageNumber + 1 > totalPages) PageNumber = totalPages - 1;
        if (PageNumber < 0) PageNumber = 0;

        if (totalPages <= 0)
            return Task.FromResult(new MenuMessage([
                new ComponentContainerProperties()
                    .WithComponents([
                        new TextDisplayProperties($"### Bag `{TargetBag.Name}`"),
                        new TextDisplayProperties("-# (bag is empty)")
                    ])
            ]));

        IEnumerable<BagEntry> pageEntries = TargetBag.Entries.Skip(PageNumber * PageEntryCount).Take(10);
        List<IComponentContainerComponentProperties> components = [];
        foreach (BagEntry entry in pageEntries)
            components.Add(new ComponentSectionProperties(
                new ButtonProperties($"menu:{MenuGuid}:delete:{entry.Id}", EmojiProperties.Custom(1417536968434389083),
                    ButtonStyle.Danger),
                [new TextDisplayProperties($"- >>> {entry.Text}")]
            ));

        if (totalPages > 1)
        {
            components.Insert(0,
                new TextDisplayProperties($"### Bag `{TargetBag.Name}`, page {PageNumber + 1} / {totalPages}"));
            components.AddRange(new ComponentSeparatorProperties(),
                new ActionRowProperties([
                    new ButtonProperties($"menu:{MenuGuid}:page-previous", EmojiProperties.Custom(1417540510003757056),
                            ButtonStyle.Secondary)
                        .WithDisabled(PageNumber + 1 <= 1),
                    new ButtonProperties($"menu:{MenuGuid}:page-next", EmojiProperties.Custom(1417540508494073876),
                            ButtonStyle.Secondary)
                        .WithDisabled(PageNumber + 1 >= totalPages)
                ]));
        }
        else
        {
            components.Insert(0, new TextDisplayProperties($"### Bag `{TargetBag.Name}`"));
        }

        return Task.FromResult(new MenuMessage([
            new ComponentContainerProperties()
                .WithComponents(components)
        ]));
    }

    public override async Task OnCreate(ApplicationCommandContext context, Guid guid)
    {
        Task deferral = RespondAsync(context, InteractionCallback.DeferredMessage());
        MenuGuid = guid;

        TerrabreakDatabaseContext db = await dbFactory.CreateDbContextAsync();
        if (db.GetBag(context.User.Id, name) is not Bag bag)
        {
            await ModifyResponseAsync(context, responseMessage => responseMessage
                .WithContent($"Bag **`{name}`** does not exist!")
                .WithFlags(MessageFlags.Ephemeral)
            );
            return;
        }

        TargetBag = bag;

        MenuMessage message = await BuildMessage();

        await deferral;
        await ModifyResponseAsync(context, responseMessage =>
            responseMessage
                .WithAttachments(message.Attachments)
                .WithComponents(message.Components)
                .WithFlags(message.Flags | MessageFlags.IsComponentsV2)
                .WithAllowedMentions(message.AllowedMentions));
    }

    public override async Task OnButton(ButtonInteractionContext context)
    {
        if (TargetBag is null)
        {
            throw new InvalidOperationException("Cannot build a bag view message without a bag to build from!");
        }

        string[] customIdFragments = context.Interaction.Data.CustomId.Split(":");
        string secondLastCustomIdFragment = customIdFragments[^2];
        string lastCustomIdFragment = customIdFragments[^1];
        if (secondLastCustomIdFragment.StartsWith("delete"))
        {
            TerrabreakDatabaseContext db = await dbFactory.CreateDbContextAsync();
            Guid deleteId = Guid.Parse(lastCustomIdFragment);
            BagEntry? entry = db.Find<BagEntry>(deleteId);
            if (entry is null) throw new InvalidOperationException("Bag entry must exist to be deleted!");
            db.Remove(entry);
            await db.SaveChangesAsync();
            TargetBag = db.Find<Bag>(TargetBag.Id);
        }
        else if (lastCustomIdFragment == "page-next")
        {
            PageNumber++;
        }
        else if (lastCustomIdFragment == "page-previous")
        {
            PageNumber--;
        }

        MenuMessage message = await BuildMessage();
        await RespondAsync(context, InteractionCallback.ModifyMessage(responseMessage => responseMessage
            .WithAttachments(message.Attachments)
            .WithComponents(message.Components)
            .WithFlags(message.Flags | MessageFlags.IsComponentsV2)
            .WithAllowedMentions(message.AllowedMentions)));
    }
}
