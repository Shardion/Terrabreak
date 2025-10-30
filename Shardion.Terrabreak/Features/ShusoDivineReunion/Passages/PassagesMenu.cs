using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Takeover;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Passages;

public class PassagesMenu(SdrServer server) : TerrabreakMenu
{
    public int PageEntryCount { get; set; } = 10;
    public int PageNumber { get; set; } = 0;
    public override Task<MenuMessage> BuildMessage()
    {
        List<IComponentContainerComponentProperties> components = [];

        // Integer division rounds towards zero
        int fullPages = server.PassagesUnlocked.Count / PageEntryCount;
        // Add page for remainder items
        bool remainderPage = server.PassagesUnlocked.Count % PageEntryCount > 0;
        int totalPages = remainderPage ? fullPages + 1 : fullPages;

        if (PageNumber + 1 > totalPages) PageNumber = totalPages - 1;
        if (PageNumber < 0) PageNumber = 0;

        if (totalPages <= 0)
        {
            return Task.FromResult(new MenuMessage([
                new ComponentContainerProperties()
                    .WithComponents([
                        new TextDisplayProperties("### Passages"),
                        new TextDisplayProperties("-# (nobody has bought any passages from the shop)")
                    ])
            ]));
        }

        int[] sortedIndices = new int[server.PassagesUnlocked.Count];
        server.PassagesUnlocked.CopyTo(sortedIndices, 0);
        sortedIndices.Sort();

        foreach (int index in sortedIndices.Skip(PageNumber * PageEntryCount).Take(10))
        {
            components.Add(new TextDisplayProperties($"{index + 1}. > {SdrRegistries.Passages[index]}"));
        }

        if (totalPages > 1)
        {
            components.Insert(0, new TextDisplayProperties($"### Passages, page {PageNumber + 1} / {totalPages}"));
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
            components.Insert(0, new TextDisplayProperties($"### Passages"));
        }

        return Task.FromResult(new MenuMessage([
            new ComponentContainerProperties(components)
        ]));
    }

    public override async Task OnButton(ButtonInteractionContext context)
    {
        string[] customIdFragments = context.Interaction.Data.CustomId.Split(":");
        string lastCustomIdFragment = customIdFragments[^1];
        if (lastCustomIdFragment == "page-next")
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
