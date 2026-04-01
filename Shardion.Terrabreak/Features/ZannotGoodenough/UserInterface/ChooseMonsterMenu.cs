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
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Machina;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Nature;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Spirit;
using Shardion.Terrabreak.Features.ZannotGoodenough.Player;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Emoji;
using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.UserInterface;

public class ChooseMonsterMenu(IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory, IdentityManager identity, EmojiManager emoji, MenuManager menu, DiscordPlayer player, Loadout loadout, int monsterSlot, TerrabreakMenu returnTo) : TerrabreakMenu
{
    private static readonly int PageEntryCount = 10;

    public IReadOnlyList<IMonster<MonsterState>> Monsters { get; } = player.UnlockedMonsterIdentifiers
        .Select<string, IMonster<MonsterState>>(id => Registries.Monsters.Forward[id])
        .ToList()
        .AsReadOnly();
    public int PageNumber { get; set; }

    public IMonster<MonsterState>? SelectedMonster { get; set; }

    public override Task<MenuMessage> BuildMessage()
    {
        if (SelectedMonster is IMonster<MonsterState> monster)
        {
            return BuildConfirmationMessage(monster);
        }

        return BuildSelectionMessage();
    }

    private Task<MenuMessage> BuildConfirmationMessage(IMonster<MonsterState> monster)
    {
        List<IComponentContainerComponentProperties> components = [
            new TextDisplayProperties($"### {IMonster<MonsterState>.ProduceClassificationIcon(emoji, monster)} {monster.Name}"),
            new TextDisplayProperties($"HP: **{monster.BaseHealth}**   ⁄ ⁄   ATK: **{monster.BaseAttack}**   ⁄ ⁄   DEF: **{monster.BaseDefense}**"),
            new TextDisplayProperties($"Classification: **{monster.Classification.ToString()}**\nCharacteristic: **{monster.Characteristic.ToString()}**"),
            new TextDisplayProperties($"Likes ability: **{monster.LikesAbility.Name}** ({monster.LikesAbility.BaseLikesCost} likes).\n{monster.LikesAbility.Description}"),
            new TextDisplayProperties($"-#   ⁄ ⁄   *'{monster.Description}'*"),
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
        int fullPages = Monsters.Count / PageEntryCount;
        // Add page for remainder entries
        bool remainderPage = Monsters.Count % PageEntryCount > 0;
        int totalPages = remainderPage ? fullPages + 1 : fullPages;

        if (PageNumber + 1 > totalPages) PageNumber = totalPages - 1;
        if (PageNumber < 0) PageNumber = 0;

        if (totalPages <= 0)
            return Task.FromResult(new MenuMessage([
                new ComponentContainerProperties()
                    .WithComponents([
                        new TextDisplayProperties("### Select Monster"),
                        new TextDisplayProperties("-# (you've unlocked no monsters)"),
                        new ComponentSeparatorProperties(),
                        new ActionRowProperties([new ButtonProperties($"menu:{MenuGuid}:back", "Back", ButtonStyle.Primary)]),
                    ])
            ]));

        IEnumerable<IMonster<MonsterState>> pageEntries = Monsters.Skip(PageNumber * PageEntryCount).Take(10);

        List<IComponentContainerComponentProperties> components = [
            new TextDisplayProperties("### Select Monster")
        ];
        foreach (IMonster<MonsterState> monster in pageEntries)
        {
            ManagedEmoji monsterIcon = IMonster<MonsterState>.ProduceClassificationIcon(emoji, monster);
            components.Add(new ComponentSectionProperties(
                new ButtonProperties($"menu:{MenuGuid}:{monster.InternalName}", "Select", ButtonStyle.Secondary),
                [new TextDisplayProperties($"- {monsterIcon} {monster.Name}")]
            ));
        }

        components.AddRange(
            new ComponentSeparatorProperties(),
            new ActionRowProperties([new ButtonProperties($"menu:{MenuGuid}:back", "Back", ButtonStyle.Secondary)])
        );

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
        if (SelectedMonster is not null)
        {
            if (splitCustomId.Last() == "confirm")
            {
                TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
                switch (monsterSlot)
                {
                    case 0:
                        loadout.Monster1Identifier = SelectedMonster.InternalName;
                        break;
                    case 1:
                        loadout.Monster2Identifier = SelectedMonster.InternalName;
                        break;
                    case 2:
                        loadout.Monster3Identifier = SelectedMonster.InternalName;
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
                SelectedMonster = null;
            }
        }
        else
        {
            string monsterId = splitCustomId.Last();
            if (player.UnlockedMonsterIdentifiers.Contains(monsterId))
            {
                SelectedMonster = Registries.Monsters.Forward[monsterId];
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
