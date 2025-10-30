using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Shop;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Takeover;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;

public class LobbyMenu(IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory, MenuManager menuManager, SdrChannel channel, TakeoverManager takeoverManager) : TerrabreakMenu
{
    public ConcurrentBag<DiscordPlayer> Players { get; set; } = [];
    public DiscordPlayer? Leader { get; set; }

    public override async Task OnCreate(ApplicationCommandContext context, Guid guid)
    {
        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        DiscordPlayer player = await dbContext.GetOrCreatePlayerFromUserAsync(context.Interaction.User);
        Leader = player;
        Players.Add(player);

        await base.OnCreate(context, guid);
    }

    public override Task<MenuMessage> BuildMessage()
    {
        if (Players.IsEmpty)
        {
            return Task.FromResult(BuildClosedMessage());
        }
        return Task.FromResult(BuildLobbyMessage());
    }


    public MenuMessage BuildLobbyMessage()
    {
        List<IComponentContainerComponentProperties> battleUi = [new TextDisplayProperties("### Lobby")];
        if (channel.TakenOver)
        {
            battleUi.Add(new TextDisplayProperties($"Join the fight to take back <#{channel.ChannelId}>!"));
        }
        else
        {
            battleUi.Add(new TextDisplayProperties("It's material grinding time!"));
        }
        string liberate = " **liberate the channel,** and";
        battleUi.Add(new TextDisplayProperties($"This channel {(channel.TakenOver ? "is" : "was")} held by **{channel.Captor.Name}**.\nIf you win, you'll{(channel.TakenOver ? liberate : "")} receive <:credit:1426414005957689445> **~{channel.Captor.Credits}**."));

        foreach (DiscordPlayer player in Players)
        {
            string emojiWeapon = ShopMenu.EmojiListsByEquipmentType[EquipmentType.Weapon][(int)player.Weapon.Tier];
            string emojiShield = ShopMenu.EmojiListsByEquipmentType[EquipmentType.Shield][(int)player.Shield.Tier];
            string emojiHeal = ShopMenu.EmojiListsByEquipmentType[EquipmentType.Heal][(int)(player.Heal?.Tier ?? Tier.Zero)];
            string emojiCure = ShopMenu.EmojiListsByEquipmentType[EquipmentType.Cure][(int)(player.Cure?.Tier ?? Tier.Zero)];

            battleUi.Add(new TextDisplayProperties($"- **{player.Name}** {emojiWeapon}{emojiShield}{emojiHeal}{emojiCure}"));
        }

        battleUi.Add(new ActionRowProperties([
            new ButtonProperties($"menu:{MenuGuid}:join", "Join", ButtonStyle.Success),
            new ButtonProperties($"menu:{MenuGuid}:leave", "Leave", ButtonStyle.Danger),
            new ButtonProperties($"menu:{MenuGuid}:start", "Start", ButtonStyle.Secondary),
        ]));

        return new MenuMessage([
            new ComponentContainerProperties(battleUi)
        ]);
    }

    public MenuMessage BuildClosedMessage()
    {
        List<IComponentContainerComponentProperties> battleUi =
        [
            new TextDisplayProperties("### Lobby"),
            new TextDisplayProperties("No players remain, so this lobby is now closed."),
            new TextDisplayProperties("-# Hint: You can start your own with `/liberate`!")
        ];

        return new MenuMessage([
            new ComponentContainerProperties(battleUi)
        ]);
    }

    public override async Task OnButton(ButtonInteractionContext context)
    {
        bool rebuild = false;

        if (context.Interaction.Data.CustomId.EndsWith("join"))
        {
            if (Players.Any(player => player.UserId == context.Interaction.User.Id))
            {
                await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                    .WithContent("You're already in this lobby!")
                    .WithFlags(MessageFlags.Ephemeral)));
                return;
            }
            TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
            Players.Add(await dbContext.GetOrCreatePlayerFromUserAsync(context.Interaction.User));
            rebuild = true;
        }
        else if (context.Interaction.Data.CustomId.EndsWith("leave"))
        {
            List<DiscordPlayer> copyPlayers = Players.ToList();
            if (copyPlayers.RemoveAll(player => player.UserId == context.Interaction.User.Id) <= 0)
            {
                await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                    .WithContent("You're not in this lobby!")
                    .WithFlags(MessageFlags.Ephemeral)));
                return;
            }

            if (Leader?.UserId == context.Interaction.User.Id)
            {
                if (copyPlayers.FirstOrDefault() is DiscordPlayer newLeader)
                {
                    Leader = newLeader;
                }
                else
                {
                    Leader = null;
                }
            }

            Players = new ConcurrentBag<DiscordPlayer>(copyPlayers);
            rebuild = true;
        }
        else if (context.Interaction.Data.CustomId.EndsWith("start"))
        {
            List<DiscordPlayer> players = Players.ToList();
            await ReplaceMenuAsync(context, menuManager, new LiberationMenu(dbContextFactory, players, new BattleEngine(channel.Captor, players), takeoverManager));
        }

        if (rebuild)
        {
            MenuMessage message = await BuildMessage();
            await RespondAsync(context, InteractionCallback.ModifyMessage(responseMessage => responseMessage
                .WithAttachments(message.Attachments)
                .WithComponents(message.Components)
                .WithFlags(message.Flags | MessageFlags.IsComponentsV2)
                .WithAllowedMentions(message.AllowedMentions)));
        }
    }
}
