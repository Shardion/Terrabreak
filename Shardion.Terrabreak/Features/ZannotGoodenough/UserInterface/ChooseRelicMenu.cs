using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using Serilog;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;
using Shardion.Terrabreak.Features.ZannotGoodenough.Player;
using Shardion.Terrabreak.Features.ZannotGoodenough.Relics;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Emoji;
using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.UserInterface;

public class ChooseRelicMenu(IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory, IdentityManager identity, EmojiManager emoji, MenuManager menu, DiscordPlayer player, Loadout loadout, int monsterSlot, TerrabreakMenu returnTo) : TerrabreakMenu
{
    private static readonly int PageEntryCount = 10;

    public IReadOnlyList<IRelic<RelicState>> Relics { get; } = player.UnlockedRelicIdentifiers
        .Where(id => !loadout.GetRelicIdentifierEnumerator().Contains(id))
        .Select<string, IRelic<RelicState>>(id => Registries.Relics.Forward[id])
        .ToList()
        .AsReadOnly();
    public int PageNumber { get; set; }

    public IRelic<RelicState>? SelectedRelic { get; set; }

    public override Task<MenuMessage> BuildMessage()
    {
        if (SelectedRelic is IRelic<RelicState> relic)
        {
            return BuildConfirmationMessage(relic);
        }

        return BuildSelectionMessage();
    }

    private Task<MenuMessage> BuildConfirmationMessage(IRelic<RelicState> relic)
    {
        List<IComponentContainerComponentProperties> components = [
            new TextDisplayProperties($"### {emoji.GetEmoji(relic.Series.EmojiIdentifier)} {relic.Name}"),
            new TextDisplayProperties($"{relic.EffectDescription}"),
            new TextDisplayProperties($"-#   ⁄ ⁄   *'{relic.Description}'*"),
            new ComponentSeparatorProperties(),
            new ActionRowProperties([
                new ButtonProperties($"menu:{MenuGuid}:confirm", "Select", ButtonStyle.Success),
                new ButtonProperties($"menu:{MenuGuid}:cancel", "Cancel", ButtonStyle.Danger),
            ])
        ];

        return Task.FromResult(new MenuMessage([
            new ComponentContainerProperties(components)
        ]));
    }

    private Task<MenuMessage> BuildSelectionMessage()
    {
        // Integer division rounds towards zero
        int fullPages = Relics.Count / PageEntryCount;
        // Add page for remainder entries
        bool remainderPage = Relics.Count % PageEntryCount > 0;
        int totalPages = remainderPage ? fullPages + 1 : fullPages;

        if (PageNumber + 1 > totalPages) PageNumber = totalPages - 1;
        if (PageNumber < 0) PageNumber = 0;

        Log.Debug("Pages: 0/{cur}/{max}.", PageNumber, totalPages - 1);

        if (totalPages <= 0)
            return Task.FromResult(new MenuMessage([
                new ComponentContainerProperties()
                    .WithComponents([
                        new TextDisplayProperties("### Select Relic"),
                        new TextDisplayProperties("-# (no applicable relics)"),
                        new ComponentSeparatorProperties(),
                        new ActionRowProperties([new ButtonProperties($"menu:{MenuGuid}:back", "Back", ButtonStyle.Primary)]),
                    ])
            ]));

        IEnumerable<IRelic<RelicState>> pageEntries = Relics.Skip(PageNumber * PageEntryCount).Take(10);

        List<IComponentContainerComponentProperties> components = [
            new TextDisplayProperties("### Select Relic")
        ];
        foreach (IRelic<RelicState> relic in pageEntries)
        {
            // TODO: Emojis for relics
            ManagedEmoji relicIcon = emoji.GetEmoji(relic.Series.EmojiIdentifier);
            components.Add(new ComponentSectionProperties(
                new ButtonProperties($"menu:{MenuGuid}:{relic.InternalName}", "Select", ButtonStyle.Secondary),
                [new TextDisplayProperties($"- {relicIcon} {relic.Name}")]
            ));
        }

        components.Add(new ComponentSeparatorProperties());
        ActionRowProperties controls = [];
        if (totalPages > 1)
        {
            controls.Add(new ButtonProperties($"menu:{MenuGuid}:page-previous", EmojiProperties.Custom(1417540510003757056),
                    ButtonStyle.Secondary)
                .WithDisabled(PageNumber + 1 <= 1));
            controls.Add(new ButtonProperties($"menu:{MenuGuid}:page-next", EmojiProperties.Custom(1417540508494073876),
                    ButtonStyle.Secondary)
                .WithDisabled(PageNumber + 1 >= totalPages));
        }
        controls.Add(new ButtonProperties($"menu:{MenuGuid}:back", "Back", ButtonStyle.Secondary));
        components.Add(controls);

        return Task.FromResult(new MenuMessage([
            new ComponentContainerProperties(components)
        ]));
    }

    public override async Task OnButton(ButtonInteractionContext context)
    {
        if (player.DiscordUserId != context.User.Id)
        {
            await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent(identity.GetAccessDeniedResponse())
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }
        List<Task> tasks = [];
        bool returnToLastMenu = false;

        string[] splitCustomId = context.Interaction.Data.CustomId.Split(':');
        if (splitCustomId.Last() == "back")
        {
            await ReplaceMenuAsync(context, menu, returnTo);
            return;
        }
        if (splitCustomId.Last() == "page-next")
        {
            PageNumber++;
            Log.Debug("Paging to {num}.", PageNumber);
            MenuMessage newMessage = await BuildMessage();
            await RespondAsync(context, InteractionCallback.ModifyMessage(responseMessage => responseMessage
                .WithAttachments(newMessage.Attachments)
                .WithComponents(newMessage.Components)
                .WithFlags(newMessage.Flags | MessageFlags.IsComponentsV2)
                .WithAllowedMentions(newMessage.AllowedMentions)));
            return;
        }
        if (splitCustomId.Last() == "page-previous")
        {
            PageNumber--;
            Log.Debug("Paging to {num}.", PageNumber);
            MenuMessage newMessage = await BuildMessage();
            await RespondAsync(context, InteractionCallback.ModifyMessage(responseMessage => responseMessage
                .WithAttachments(newMessage.Attachments)
                .WithComponents(newMessage.Components)
                .WithFlags(newMessage.Flags | MessageFlags.IsComponentsV2)
                .WithAllowedMentions(newMessage.AllowedMentions)));
            return;
        }
        if (SelectedRelic is not null)
        {
            if (splitCustomId.Last() == "confirm")
            {
                TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
                switch (monsterSlot)
                {
                    case 0:
                        loadout.Relic1Identifier = SelectedRelic.InternalName;
                        break;
                    case 1:
                        loadout.Relic2Identifier = SelectedRelic.InternalName;
                        break;
                    case 2:
                        loadout.Relic3Identifier = SelectedRelic.InternalName;
                        break;
                    case 3:
                        loadout.Relic4Identifier = SelectedRelic.InternalName;
                        break;
                    case 4:
                        loadout.Relic5Identifier = SelectedRelic.InternalName;
                        break;
                    case 5:
                        loadout.Relic6Identifier = SelectedRelic.InternalName;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                dbContext.Update(player);
                dbContext.Update(loadout);
                tasks.Add(dbContext.SaveChangesAsync());
                returnToLastMenu = true;
            }
            else
            {
                SelectedRelic = null;
            }
        }
        else
        {
            string relicId = splitCustomId.Last();
            if (player.UnlockedRelicIdentifiers.Contains(relicId))
            {
                SelectedRelic = Registries.Relics.Forward[relicId];
            }
        }

        MenuMessage message = await BuildMessage();
        if (returnToLastMenu)
        {
            tasks.Add(ReplaceMenuAsync(context, menu, returnTo));
        }
        else
        {
            tasks.Add(RespondAsync(context, InteractionCallback.ModifyMessage(responseMessage => responseMessage
                .WithAttachments(message.Attachments)
                .WithComponents(message.Components)
                .WithFlags(message.Flags | MessageFlags.IsComponentsV2)
                .WithAllowedMentions(message.AllowedMentions))));
        }


        await Task.WhenAll(tasks);
    }
}
