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
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;
using Shardion.Terrabreak.Features.ZannotGoodenough.Player;
using Shardion.Terrabreak.Features.ZannotGoodenough.Relics;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Emoji;
using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.UserInterface;

public class EditLoadoutMenu(IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory, IdentityManager identity, EmojiManager emoji, MenuManager menu, DiscordPlayer player, Loadout loadout, TerrabreakMenu returnTo) : TerrabreakMenu
{
    public EditLoadoutMenuMode Mode { get; set; } = EditLoadoutMenuMode.Monsters;

    public override Task<MenuMessage> BuildMessage()
    {
        List<IComponentContainerComponentProperties> components = [];
        components.AddRange(Mode switch
        {
            EditLoadoutMenuMode.Monsters => BuildMonsterList(),
            EditLoadoutMenuMode.Relics => BuildRelicList(),
            _ => throw new ArgumentOutOfRangeException()
        });

        components.Add(new ComponentSeparatorProperties());

        ActionRowProperties controlsRow = [];
        controlsRow.Add(Mode switch
        {
            EditLoadoutMenuMode.Monsters => new($"menu:{MenuGuid}:relics", "Relics", ButtonStyle.Primary),
            EditLoadoutMenuMode.Relics => new ButtonProperties($"menu:{MenuGuid}:monsters", "Monsters", ButtonStyle.Primary),
            _ => throw new ArgumentOutOfRangeException()
        });
        controlsRow.Add(new ButtonProperties($"menu:{MenuGuid}:back", "Back", ButtonStyle.Secondary));
        components.Add(controlsRow);

        return Task.FromResult(new MenuMessage([new ComponentContainerProperties(components)]));
    }

    private List<IComponentContainerComponentProperties> BuildMonsterList()
    {
        List<IComponentContainerComponentProperties> components = [
            new TextDisplayProperties("### Edit Loadout Monsters"),
        ];

        for (int monsterIndex = 0; monsterIndex < loadout.MonsterIdentifiers.Length; monsterIndex++)
        {
            string? monsterId = loadout.MonsterIdentifiers[monsterIndex];
            if (monsterId is not null)
            {
                IMonster<MonsterState> monster = Registries.Monsters.Forward[monsterId];
                components.Add(new ComponentSectionProperties(
                    new ButtonProperties($"menu:{MenuGuid}:monster:{monsterIndex}", "Edit", ButtonStyle.Secondary),
                    [
                        new TextDisplayProperties(
                            $"{monsterIndex + 1}. {IMonster<MonsterState>.ProduceClassificationIcon(emoji, monster)} {monster.Name}")
                    ]
                ));
            }
            else
            {
                components.Add(new ComponentSectionProperties(
                    new ButtonProperties($"menu:{MenuGuid}:monster:{monsterIndex}", "Edit", ButtonStyle.Secondary),
                    [new TextDisplayProperties($"-# {monsterIndex + 1}. {emoji.GetEmoji("noitem")} (no monster)")]
                ));
            }
        }

        return components;
    }

    private List<IComponentContainerComponentProperties> BuildRelicList()
    {
        List<IComponentContainerComponentProperties> components = [
            new TextDisplayProperties("### Edit Loadout Relics"),
        ];


        for (int relicIndex = 0; relicIndex < loadout.RelicIdentifiers.Length; relicIndex++)
        {
            string? relicId = loadout.RelicIdentifiers[relicIndex];
            if (relicId is not null)
            {
                IRelic<RelicState> relic = Registries.Relics.Forward[relicId];
                components.Add(new ComponentSectionProperties(
                    new ButtonProperties($"menu:{MenuGuid}:relic:{relicIndex}", "Edit", ButtonStyle.Secondary),
                    [
                        new TextDisplayProperties(
                            $"{relicIndex + 1}. {emoji.GetEmoji(relic.Series.EmojiIdentifier)} {relic.Name}")
                    ]
                ));
            }
            else
            {
                components.Add(new ComponentSectionProperties(
                    new ButtonProperties($"menu:{MenuGuid}:relic:{relicIndex}", "Edit", ButtonStyle.Secondary),
                    [new TextDisplayProperties($"-# {relicIndex + 1}. {emoji.GetEmoji("noitem")} (no relic)")]
                ));
            }
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
            Mode = splitCustomId[2] switch
            {
                "monsters" => EditLoadoutMenuMode.Monsters,
                "relics" => EditLoadoutMenuMode.Relics,
                _ => Mode,
            };
        }
        else if (splitCustomId.Length == 4)
        {
            int selectedThing = int.Parse(splitCustomId[3]);
            if (splitCustomId[2] == "monster")
            {
                await ReplaceMenuAsync(context, menu, new ChooseMonsterMenu(dbContextFactory, identity, emoji, menu, player, loadout, selectedThing, this));
                return;
            }

            if (splitCustomId[2] == "relic")
            {
                await ReplaceMenuAsync(context, menu, new ChooseRelicMenu(dbContextFactory, identity, emoji, menu, player, loadout, selectedThing, this));
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
