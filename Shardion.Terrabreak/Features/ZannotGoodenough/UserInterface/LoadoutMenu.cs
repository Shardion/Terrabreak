using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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

public class LoadoutMenu(IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory, IdentityManager identity, EmojiManager emoji, MenuManager menu, DiscordPlayer player) : TerrabreakMenu
{
    public LoadoutMenuMode Mode { get; set; } = LoadoutMenuMode.SwitchMode;

    public override Task<MenuMessage> BuildMessage()
    {
        List<IComponentContainerComponentProperties> components = [
            new TextDisplayProperties("### Loadouts"),
        ];

        List<Loadout> loadouts = [player.Loadout1, player.Loadout2, player.Loadout3];
        for (int index = 0; index < loadouts.Count; index++)
        {
            Loadout loadout = loadouts[index];
            components.Add(ProduceLoadoutLine(loadout, index, player.EquippedLoadoutIndex == index));
        }

        components.Add(new ComponentSeparatorProperties());

        ActionRowProperties modeSwitchRow = [];
        if (Mode == LoadoutMenuMode.EditMode)
        {
            modeSwitchRow.Add(new ButtonProperties($"menu:{MenuGuid}:switch", "Stop Editing", ButtonStyle.Secondary));
        }
        else
        {
            modeSwitchRow.Add(new ButtonProperties($"menu:{MenuGuid}:edit", "Edit", ButtonStyle.Secondary));
        }
        components.Add(modeSwitchRow);

        return Task.FromResult(new MenuMessage([
            new ComponentContainerProperties(components)
        ]));
    }

    private ComponentSectionProperties ProduceLoadoutLine(Loadout loadout, int id, bool active)
    {
        string line = $"{id + 1}. {Loadout.ProduceLoadoutLine(emoji, loadout)}";

        if (Mode == LoadoutMenuMode.EditMode)
        {
            return new(
                new ButtonProperties($"menu:{MenuGuid}:edit:{id.ToString(CultureInfo.InvariantCulture)}", "Edit",
                    ButtonStyle.Secondary),
                [new TextDisplayProperties(line)]
            );
        }

        if (active)
        {
            return new(
                new ButtonProperties($"menu:{MenuGuid}:switch:{id.ToString(CultureInfo.InvariantCulture)}", "Active",
                    ButtonStyle.Secondary)
                {
                    Disabled = true
                },
                [new TextDisplayProperties(line)]
            );
        }
        return new(
            new ButtonProperties($"menu:{MenuGuid}:switch:{id.ToString(CultureInfo.InvariantCulture)}", "Select",
                ButtonStyle.Secondary),
            [new TextDisplayProperties(line)]
        );
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
        string[] splitCustomId = context.Interaction.Data.CustomId.Split(':');
        if (splitCustomId.Length == 3)
        {
            Mode = splitCustomId[2] switch
            {
                "edit" => LoadoutMenuMode.EditMode,
                "switch" => LoadoutMenuMode.SwitchMode,
                _ => Mode,
            };
        }
        else if (splitCustomId.Length == 4)
        {
            LoadoutMenuMode mode = splitCustomId[2] switch
            {
                "edit" => LoadoutMenuMode.EditMode,
                "switch" => LoadoutMenuMode.SwitchMode,
                _ => Mode,
            };
            int selectedLoadout = int.Parse(splitCustomId[3]);
            Loadout target = selectedLoadout switch
            {
                0 => player.Loadout1,
                1 => player.Loadout2,
                2 => player.Loadout3,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (mode == LoadoutMenuMode.SwitchMode)
            {
                player.EquippedLoadoutIndex = selectedLoadout;

                TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
                dbContext.Update(player);
                tasks.Add(dbContext.SaveChangesAsync());
            }
            else if (mode == LoadoutMenuMode.EditMode)
            {
                await ReplaceMenuAsync(context, menu, new EditLoadoutMenu(dbContextFactory, identity, emoji, menu, player, target, this));
                return;
            }
        }

        MenuMessage message = await BuildMessage();
        tasks.Add(RespondAsync(context, InteractionCallback.ModifyMessage(responseMessage => responseMessage
            .WithAttachments(message.Attachments)
            .WithComponents(message.Components)
            .WithFlags(message.Flags | MessageFlags.IsComponentsV2)
            .WithAllowedMentions(message.AllowedMentions))));

        await Task.WhenAll(tasks);
    }
}
