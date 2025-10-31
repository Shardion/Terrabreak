using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Gateway;
using NetCord.JsonModels;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Shardion.Terrabreak.Features.Documentation;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Enemies;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Passages;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Shop;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Takeover;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion;

public class LiberationModule(MenuManager menuManager, IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory, IdentityManager identityManager, TakeoverManager takeoverManager, OverviewManager overviewManager, LiberationFeature liberationFeature)
    : TerrabreakApplicationCommandModule(menuManager)
{
    private readonly MenuManager _menuManager = menuManager;

    [TakeoverStartedPrecondition<ApplicationCommandContext>]
    [SlashCommand("shop", "Gear up to fight the invaders!", Contexts = [InteractionContextType.Guild])]
    public async Task Shop()
    {
        await ActivateMenuAsync(new ShopMenu(dbContextFactory, identityManager)
        {
            AllowedUsers = new HashSet<ulong>([Context.Interaction.User.Id])
        });
    }

    [TakeoverStartedPrecondition<ApplicationCommandContext>]
    [SlashCommand("inventory", "Look at your amazing gear!!", Contexts = [InteractionContextType.Guild])]
    public async Task Inventory()
    {
        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        DiscordPlayer player = await dbContext.GetOrCreatePlayerFromUserAsync(Context.User);
        await ActivateMenuAsync(new InventoryMenu(player)
        {
            AllowedUsers = new HashSet<ulong>([Context.Interaction.User.Id])
        });
    }

    [TakeoverStartedPrecondition<ApplicationCommandContext>]
    [SlashCommand("liberate", "Rid this channel of its invaders! Liberation, yay!!", Contexts = [InteractionContextType.Guild])]
    public async Task Liberate()
    {
        if (Context.Interaction.Guild is not Guild server)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("This command must be used inside a server! (Otherwise, where would the monsters live...?)")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        if (liberationFeature.PlayersInBattles.ContainsKey(Context.User.Id))
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("You're already in a battle!")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        // Sad and awful hack, but I forgot I needed to do this...!!
        if (Context.Channel.Id == 991003051160662086 || Context.Channel.Id == 1433458371989864452)
        {
            await ActivateMenuAsync(new SecretBossMenu(dbContextFactory, _menuManager, server, takeoverManager, liberationFeature));
            return;
        }

        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        if (dbContext.GetChannel(Context.Channel.Id) is not SdrChannel channel)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("This channel hasn't been taken over!")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        if (IEnemy.GetMaxPlayers(channel.Captor) <= 1)
        {
            DiscordPlayer player = await dbContext.GetOrCreatePlayerFromUserAsync(Context.Interaction.User);
            await ActivateMenuAsync(new LiberationMenu(dbContextFactory, [player], new BattleEngine(channel.Captor, [player]), takeoverManager, liberationFeature));
            return;
        }

        await ActivateMenuAsync(new LobbyMenu(dbContextFactory, _menuManager, channel, takeoverManager, liberationFeature));
    }

    [TakeoverStartedPrecondition<ApplicationCommandContext>]
    [SlashCommand("gift", "Give away Credits to someone else!", Contexts = [InteractionContextType.Guild])]
    public async Task Gift(int credits, User receivingUser)
    {
        if (receivingUser.Id == Context.Interaction.User.Id)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("You can't give a gift to yourself! Or, maybe, it's just about framing...?")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }
        if (credits == 0)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("You can't give nothing! That would be a pretty lousy gift...")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }
        if (credits < 0)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("You can't give in negatives! That would be theft...")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }
        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        DiscordPlayer giftingPlayer = await dbContext.GetOrCreatePlayerFromUserAsync(Context.Interaction.User);
        DiscordPlayer receivingPlayer = await dbContext.GetOrCreatePlayerFromUserAsync(receivingUser);
        if (giftingPlayer.Credits < credits)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent($"You are missing <:credit:1426414005957689445> **{(credits - giftingPlayer.Credits).ToString(CultureInfo.InvariantCulture)}**!")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        giftingPlayer.Credits -= credits;
        receivingPlayer.Credits += credits;
        dbContext.UpdateRange(giftingPlayer, receivingPlayer);
        Task saveTask = dbContext.SaveChangesAsync();
        Task respondTask = RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            .WithComponents([new ComponentContainerProperties([
                new TextDisplayProperties("### Gift"),
                new TextDisplayProperties($"<@{Context.Interaction.User.Id}> has given <:credit:1426414005957689445> **{credits}** to <@{receivingUser.Id}>!"),
                new ComponentSeparatorProperties(),
                new TextDisplayProperties($"-# <:credit:1426414005957689445> {giftingPlayer.Credits} <@{giftingPlayer.UserId}>"),
                new TextDisplayProperties($"-# <:credit:1426414005957689445> {receivingPlayer.Credits} <@{receivingPlayer.UserId}>"),
            ])])
            .WithFlags(MessageFlags.IsComponentsV2)
        ));
        await Task.WhenAll(saveTask, respondTask);
    }

    [TakeoverStartedPrecondition<ApplicationCommandContext>]
    [SlashCommand("next", "See which invaders to fight next!", Contexts = [InteractionContextType.Guild])]
    public async Task Next()
    {
        if (Context.Interaction.Guild is not Guild server)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("This command must be used inside a server! (Otherwise, where would the monsters live...?)")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }
        List<IComponentContainerComponentProperties> components = [
            new TextDisplayProperties("### Next")
        ];

        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        DiscordPlayer player = await dbContext.GetOrCreatePlayerFromUserAsync(Context.Interaction.User);
        double playerStrongestTpl = player.StrongestEnemy?.Enemy.TargetTotalPowerLevel ?? 0.0;

        if (player.StrongestEnemy is EnemyRecord strongestEnemy)
        {
            components.Add(new TextDisplayProperties($"The strongest invader you've defeated is **{strongestEnemy.Enemy.Name}**, in <#{strongestEnemy.SourceChannelId}>."));
            components.Add(new ComponentSeparatorProperties());
        }

        IEnumerable<SdrChannel> channels = dbContext.Set<SdrChannel>().Where(channel => channel.ServerId == server.Id).AsEnumerable().Where(channel => channel.Captor.TargetTotalPowerLevel > playerStrongestTpl);
        SortedSet<SdrChannel> sortedChannels = new(channels, new SdrChannelTplComparer());
        SdrChannel? maybeChannel = sortedChannels.FirstOrDefault();
        if (maybeChannel is SdrChannel channel)
        {
            components.Add(new TextDisplayProperties(
                $"The next invader to defeat is **{channel.Captor.Name}**, in <#{channel.ChannelId}>."));
        }
        else
        {
            components.Add(new TextDisplayProperties(
                "You've defeated all the invaders!!"));
        }

        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            .WithComponents([new ComponentContainerProperties(components)])
            .WithFlags(MessageFlags.IsComponentsV2)
        ));
    }

    [TakeoverStartedPrecondition<ApplicationCommandContext>]
    [SlashCommand("passages", "Learn... something!!", Contexts = [InteractionContextType.Guild])]
    public async Task Passages()
    {
        if (Context.Interaction.Guild is not Guild server)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("This command must be used inside a server! (Otherwise, how would you share the passages...?)")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        SdrServer sdrServer = await dbContext.GetOrCreateServerAsync(server.Id);
        await ActivateMenuAsync(new PassagesMenu(sdrServer)
        {
            AllowedUsers = new HashSet<ulong>([Context.Interaction.User.Id])
        });
    }

    [TakeoverStartedPrecondition<ApplicationCommandContext>]
    [SlashCommand("overview", "Learn about the takeover!")]
    public Task Overview()
    {
        if (overviewManager.SdrOverview is not JsonComponent component)
        {
            return RespondAsync(InteractionCallback.Message(new InteractionMessageProperties
                {
                    Content = "TODO: Add SDR overview —S",
                    Flags = MessageFlags.Ephemeral
                }
            ));
        }

        return RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            .WithComponents([
                new DumbComponent(component)
            ])
            .WithFlags(MessageFlags.IsComponentsV2)
        ));
    }

    [TakeoverStartedPrecondition<ApplicationCommandContext>]
    [SlashCommand("reset", "Restart this rotten earth.", Contexts = [InteractionContextType.Guild], IntegrationTypes = [ApplicationIntegrationType.GuildInstall])]
    public async Task Reset()
    {
        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        DiscordPlayer player = await dbContext.GetOrCreatePlayerFromUserAsync(Context.Interaction.User);

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
}
