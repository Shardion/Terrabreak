using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.ClearRateTest;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Shop;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Takeover;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion;

[InstanceOwnerPrecondition<ApplicationCommandContext>]
public class SdrDebugModule(MenuManager menuManager, TakeoverManager takeoverManager, IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory, LiberationFeature liberationFeature)
    : TerrabreakApplicationCommandModule(menuManager)
{
    [InstanceOwnerPrecondition<ApplicationCommandContext>]
    [SlashCommand("teto-punch", "Fight an enemy.", Contexts = [InteractionContextType.Guild], IntegrationTypes = [ApplicationIntegrationType.GuildInstall],
        DefaultGuildPermissions = Permissions.Administrator)]
    public async Task TetoPunch(string enemyId, int extraPlayerNum = 0, string helperWeaponId = "WoodenBlade", string helperShieldId = "CardboardShield", string helperHealId = "Sandwich", string helperCureId = "Bread")
    {
        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        DiscordPlayer player = await dbContext.GetOrCreatePlayerFromUserAsync(Context.Interaction.User);
        List<IPlayer> players = [player];

        IEnemy enemy = SdrRegistries.Enemies.Forward[enemyId];
        IWeapon weapon = SdrRegistries.Weapons.Forward[helperWeaponId];
        IShield shield = SdrRegistries.Shields.Forward[helperShieldId];
        IHeal heal = SdrRegistries.Heals.Forward[helperHealId];
        ICure cure = SdrRegistries.Cures.Forward[helperCureId];
        for (int i = 0; i < extraPlayerNum; i++)
        {
            string name = "Mister Helper";
            if (i == 0)
            {
                name = "\"The Singer\"";
            }
            else if (i == 1)
            {
                name = "\"Twinespinner\"";
            }
            else if (i == 2)
            {
                name = "\"Parting\"";
            }

            players.Add(new HelperPlayer
            {
                Name = name,
                Weapon = weapon,
                Shield = shield,
                Heal = heal,
                Cure = cure,
            });
        }

        await ActivateMenuAsync(new LiberationMenu(dbContextFactory, [player], new BattleEngine(enemy, players), takeoverManager, liberationFeature));
    }

    [InstanceOwnerPrecondition<ApplicationCommandContext>]
    [SlashCommand("teto-cash", "Get Credits.", Contexts = [InteractionContextType.Guild], IntegrationTypes = [ApplicationIntegrationType.GuildInstall],
        DefaultGuildPermissions = Permissions.Administrator)]
    public async Task TetoCash(int money)
    {
        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        DiscordPlayer player = await dbContext.GetOrCreatePlayerFromUserAsync(Context.Interaction.User);
        player.Credits += money;
        await Task.WhenAll(dbContext.SaveChangesAsync(), RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            .WithContent($"Granted <:credit:1426414005957689445> **{money}**. New balance: <:credit:1426414005957689445> {player.Credits}.")
            .WithFlags(MessageFlags.Ephemeral)
        )));
    }

    [InstanceOwnerPrecondition<ApplicationCommandContext>]
    [SlashCommand("teto-resources", "Get Ribbons.", Contexts = [InteractionContextType.Guild], IntegrationTypes = [ApplicationIntegrationType.GuildInstall],
        DefaultGuildPermissions = Permissions.Administrator)]
    public async Task TetoResources(int ribbons)
    {
        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        DiscordPlayer player = await dbContext.GetOrCreatePlayerFromUserAsync(Context.Interaction.User);
        player.Ribbons += ribbons;
        await Task.WhenAll(dbContext.SaveChangesAsync(), RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            .WithContent($"Granted <:ribbon:1429603809755398154> **{ribbons}**. New amount: <:ribbon:1429603809755398154> {player.Ribbons}.")
            .WithFlags(MessageFlags.Ephemeral)
        )));
    }

    [InstanceOwnerPrecondition<ApplicationCommandContext>]
    [SlashCommand("teto-reset", "Reset your player data.", Contexts = [InteractionContextType.Guild], IntegrationTypes = [ApplicationIntegrationType.GuildInstall],
        DefaultGuildPermissions = Permissions.Administrator)]
    public async Task TetoReset(User? user)
    {
        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        DiscordPlayer player;
        if (user is not null)
        {
            player = await dbContext.GetOrCreatePlayerFromUserAsync(user);
        }
        else
        {
            player = await dbContext.GetOrCreatePlayerFromUserAsync(Context.Interaction.User);
        }

        player.WeaponId = "WoodenBlade";
        player.ShieldId = "CardboardShield";
        player.HealId = null;
        player.CureId = null;
        player.Credits = 0;
        player.Ribbons = 0;
        player.StrongestEnemy = null;

        await Task.WhenAll(
        [
            dbContext.SaveChangesAsync(),
            RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("Reset.")
                .WithFlags(MessageFlags.Ephemeral)
            ))
        ]);
    }

    [InstanceOwnerPrecondition<ApplicationCommandContext>]
    [SlashCommand("teto-takeover", "Take over a channel.", Contexts = [InteractionContextType.Guild], IntegrationTypes = [ApplicationIntegrationType.GuildInstall],
        DefaultGuildPermissions = Permissions.Administrator)]
    public async Task TetoTakeover(IGuildChannel channel, string enemyId)
    {
        await Task.WhenAll(
            RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent($"Took over <#{channel.Id}>.")
                .WithFlags(MessageFlags.Ephemeral)
            )),
            takeoverManager.TakeoverChannelImmediatelyAsync(channel.Id, enemyId)
        );
    }

    [InstanceOwnerPrecondition<ApplicationCommandContext>]
    [SlashCommand("teto-relinquish", "Relinquish a channel.", Contexts = [InteractionContextType.Guild], IntegrationTypes = [ApplicationIntegrationType.GuildInstall],
        DefaultGuildPermissions = Permissions.Administrator)]
    public async Task TetoRelinquish(IGuildChannel channel)
    {
        await Task.WhenAll(
            RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent($"Relinquished <#{channel.Id}>.")
                .WithFlags(MessageFlags.Ephemeral)
            )),
            takeoverManager.RelinquishChannelImmediatelyAsync(channel.Id)
        );
    }

    [InstanceOwnerPrecondition<ApplicationCommandContext>]
    [SlashCommand("teto-forget", "Forget a channel.", Contexts = [InteractionContextType.Guild], IntegrationTypes = [ApplicationIntegrationType.GuildInstall],
        DefaultGuildPermissions = Permissions.Administrator)]
    public async Task TetoForget(IGuildChannel channel)
    {
        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        if (dbContext.GetChannel(channel.Id) is not SdrChannel sdrChannel)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("That channel is not known!")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }
        dbContext.Remove(sdrChannel);
        await Task.WhenAll(
            RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent($"Forgot <#{channel.Id}>.")
                .WithFlags(MessageFlags.Ephemeral)
            )),
            dbContext.SaveChangesAsync()
        );
    }

    [InstanceOwnerPrecondition<ApplicationCommandContext>]
    [SlashCommand("teto-crt", "Run a clear rate test.", Contexts = [InteractionContextType.Guild], IntegrationTypes = [ApplicationIntegrationType.GuildInstall],
        DefaultGuildPermissions = Permissions.Administrator)]
    public async Task TetoCrt(int battles, string enemyId, bool useHeal = false, int playerNum = 1, string helperWeaponId = "WoodenBlade", string helperShieldId = "CardboardShield", string helperHealId = "Sandwich", string helperCureId = "Bread")
    {
        Task deferTask = RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        IEnemy enemy = SdrRegistries.Enemies.Forward[enemyId];
        IWeapon weapon = SdrRegistries.Weapons.Forward[helperWeaponId];
        IShield shield = SdrRegistries.Shields.Forward[helperShieldId];
        IHeal heal = SdrRegistries.Heals.Forward[helperHealId];
        ICure cure = SdrRegistries.Cures.Forward[helperCureId];
        HelperPlayer basePlayer = new()
        {
            Name = "Kasane Teto",
            Weapon = weapon,
            Shield = shield,
            Heal = heal,
            Cure = cure,
        };

        ClearRateTester clearRateTester = new(enemy, playerNum, basePlayer, battles, useHeal);
        Task<IReadOnlyCollection<BattleResult>> clearRateTestTask = clearRateTester.RunTestAsync();

        await Task.WhenAll(
            deferTask,
            clearRateTestTask
        );

        IReadOnlyCollection<BattleResult> results = await clearRateTestTask;
        int clearNumber = results.Count(result => result.Victor == Victor.Players);
        int failNumber = results.Count(result => result.Victor == Victor.Enemies);
        int timeoutNumber = results.Count - clearNumber - failNumber;
        decimal clearPercent = Math.Round((decimal)clearNumber / battles * 100);
        decimal failPercent = Math.Round((decimal)failNumber / battles * 100);
        decimal timeoutPercent = Math.Round((decimal)timeoutNumber / battles * 100);

        string emojiWeapon = ShopMenu.EmojiListsByEquipmentType[EquipmentType.Weapon][(int)basePlayer.Weapon.Tier];
        string emojiShield = ShopMenu.EmojiListsByEquipmentType[EquipmentType.Shield][(int)basePlayer.Shield.Tier];
        string emojiHeal = ShopMenu.EmojiListsByEquipmentType[EquipmentType.Heal][(int)(basePlayer.Heal?.Tier ?? Tier.Zero)];
        string emojiCure = ShopMenu.EmojiListsByEquipmentType[EquipmentType.Cure][(int)(basePlayer.Cure?.Tier ?? Tier.Zero)];

        await ModifyResponseAsync(message => message
            .WithComponents([
                new ComponentContainerProperties([
                    new TextDisplayProperties("### Clear Rate Test"),
                    new TextDisplayProperties($"Of {results.Count} battles, each with {playerNum} player{(playerNum > 1 ? "s" : "")}, wielding {emojiWeapon}{emojiShield}{emojiHeal}{emojiCure}..."),
                    new TextDisplayProperties($"""
                                               - **{clearNumber}**, or **{clearPercent.ToString(CultureInfo.CurrentUICulture)}%** ended in success
                                               - **{failNumber}**, or **{failPercent.ToString(CultureInfo.CurrentUICulture)}%** ended in failure
                                               - **{timeoutNumber}**, or **{timeoutPercent.ToString(CultureInfo.CurrentUICulture)}%** did not end in 100 turns
                                               """)
                ])
            ])
            .WithFlags(MessageFlags.IsComponentsV2 | MessageFlags.Ephemeral)
        );
    }

    [InstanceOwnerPrecondition<ApplicationCommandContext>]
    [SlashCommand("teto-start", "Take over the server!", Contexts = [InteractionContextType.Guild], IntegrationTypes = [ApplicationIntegrationType.GuildInstall],
        DefaultGuildPermissions = Permissions.Administrator)]
    public async Task TetoStart(bool force = false)
    {
        if (Context.Guild is not Guild guild)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("Cannot take over no server!")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }
        await Task.WhenAll(
            RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("Taking over the server...")
                .WithFlags(MessageFlags.Ephemeral)
            )),
            takeoverManager.TakeoverServerAsync(Context.Guild.Id, force)
        );
    }

    [InstanceOwnerPrecondition<ApplicationCommandContext>]
    [SlashCommand("teto-chronos-set", "Override when the takeover is considered to have happened.", Contexts = [InteractionContextType.Guild], IntegrationTypes = [ApplicationIntegrationType.GuildInstall],
        DefaultGuildPermissions = Permissions.Administrator)]
    public async Task TetoChronosForce(string time)
    {
        takeoverManager.TakeoverTimestamp = DateTimeOffset.Parse(time);
        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            .WithContent($"Set takeover time to <t:{takeoverManager.TakeoverTimestamp.ToUnixTimeSeconds()}>.")
            .WithFlags(MessageFlags.Ephemeral)
        ));
    }

    [InstanceOwnerPrecondition<ApplicationCommandContext>]
    [SlashCommand("teto-chronos-when", "Find out how much time is left until the takeover.", Contexts = [InteractionContextType.Guild], IntegrationTypes = [ApplicationIntegrationType.GuildInstall],
        DefaultGuildPermissions = Permissions.Administrator)]
    public async Task TetoChronosWhen()
    {
        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            .WithContent($"Takeover time is <t:{takeoverManager.TakeoverTimestamp.ToUnixTimeSeconds()}>, which will occur <t:{takeoverManager.TakeoverTimestamp.ToUnixTimeSeconds()}:R>.")
            .WithFlags(MessageFlags.Ephemeral)
        ));
    }

    [InstanceOwnerPrecondition<ApplicationCommandContext>]
    [SlashCommand("teto-eliminate", "Reset the server's data.", Contexts = [InteractionContextType.Guild], IntegrationTypes = [ApplicationIntegrationType.GuildInstall],
        DefaultGuildPermissions = Permissions.Administrator)]
    public async Task TetoEliminate()
    {
        if (Context.Guild is not Guild guild)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("Cannot take over no server!")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        SdrServer server = await dbContext.GetOrCreateServerAsync(guild.Id);

        server.PassagesUnlocked = [];
        server.TakenOver = false;
        dbContext.Update(server);

        await Task.WhenAll(
        [
            dbContext.SaveChangesAsync(),
            RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("Reset.")
                .WithFlags(MessageFlags.Ephemeral)
            ))
        ]);
    }

    [InstanceOwnerPrecondition<ApplicationCommandContext>]
    [SlashCommand("teto-revive", "Take over a channel that's been liberated before.", Contexts = [InteractionContextType.Guild], IntegrationTypes = [ApplicationIntegrationType.GuildInstall],
        DefaultGuildPermissions = Permissions.Administrator)]
    public async Task TetoRevive(Channel channel)
    {
        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        SdrChannel? sdrChannel = dbContext.GetChannel(channel.Id);
        if (sdrChannel is not SdrChannel { TakenOver: false })
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("That channel has not been taken over, or its enemy has not been defeated.")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        sdrChannel.TakenOver = true;
        dbContext.Update(sdrChannel);

        await Task.WhenAll(
        [
            dbContext.SaveChangesAsync(),
            RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("Revived.")
                .WithFlags(MessageFlags.Ephemeral)
            ))
        ]);
    }
}
