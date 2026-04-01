using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using Serilog;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;
using Shardion.Terrabreak.Features.ZannotGoodenough.Player;
using Shardion.Terrabreak.Features.ZannotGoodenough.Relics;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Emoji;
using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.UserInterface;

public class EditBattleLoadoutMenu(IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory, IdentityManager identity, EmojiManager emoji, MenuManager menu, DiscordPlayer player, BattleLoadout loadout, TerrabreakMenu returnTo) : TerrabreakMenu
{
    public override Task<MenuMessage> BuildMessage()
    {
        List<IComponentContainerComponentProperties> components = [];
        components.AddRange(BuildRelicList());

        components.Add(new ComponentSeparatorProperties());

        ActionRowProperties controlsRow = [];
        controlsRow.Add(new ButtonProperties($"menu:{MenuGuid}:back", "Back", ButtonStyle.Secondary));
        components.Add(controlsRow);

        return Task.FromResult(new MenuMessage([new ComponentContainerProperties(components)]));
    }

    private List<IComponentContainerComponentProperties> BuildRelicList()
    {
        List<IComponentContainerComponentProperties> components = [
            new TextDisplayProperties("### Edit Battle Relics"),
        ];

        List<BattleRelic> relics = loadout.GetRelicEnumerator().ToList();
        int relicsAdded = 0;
        foreach (BattleRelic relic in relics)
        {
            components.Add(new ComponentSectionProperties(
                new ButtonProperties($"menu:{MenuGuid}:relic:{relicsAdded}", "Edit", ButtonStyle.Secondary),
                [new TextDisplayProperties($"{relicsAdded + 1}. {emoji.GetEmoji(relic.Relic.Series.EmojiIdentifier)} {relic.Relic.Name}")]
            ));
            relicsAdded++;
        }
        for (int relicFillIndex = relicsAdded; relicFillIndex < 4; relicFillIndex++)
        {
            components.Add(new ComponentSectionProperties(
                new ButtonProperties($"menu:{MenuGuid}:relic:{relicFillIndex}", "Edit", ButtonStyle.Secondary),
                [new TextDisplayProperties($"-# {relicFillIndex + 1}. {emoji.GetEmoji("noitem")} (no relic)")]
            ));
        }

        return components;
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
        string[] splitCustomId = context.Interaction.Data.CustomId.Split(':');
        if (splitCustomId.Length == 3)
        {
            if (splitCustomId[2] == "back")
            {
                await ReplaceMenuAsync(context, menu, returnTo);
                return;
            }
        }
        else if (splitCustomId.Length == 4)
        {
            int selectedThing = int.Parse(splitCustomId[3]);
            if (splitCustomId[2] == "relic")
            {
                await ReplaceMenuAsync(context, menu, new ChooseBattleRelicMenu(dbContextFactory, identity, emoji, menu, player, loadout, selectedThing, this));
                return;
            }
        }

        MenuMessage message = await BuildMessage();
        await RespondAsync(context, InteractionCallback.ModifyMessage(responseMessage => responseMessage
            .WithAttachments(message.Attachments)
            .WithComponents(message.Components)
            .WithFlags(message.Flags | MessageFlags.IsComponentsV2)
            .WithAllowedMentions(message.AllowedMentions)));
    }
}
