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
using Shardion.Terrabreak.Features.ShusoDivineReunion.Passages;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Shop;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Takeover;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion;

public class LiberationModule(MenuManager menuManager, IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory, IdentityManager identityManager, TakeoverManager takeoverManager, OverviewManager overviewManager)
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
        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        if (dbContext.GetChannel(Context.Channel.Id) is not SdrChannel channel)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("This channel hasn't been taken over!")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        await ActivateMenuAsync(new LobbyMenu(dbContextFactory, _menuManager, channel, takeoverManager));
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

        components.Add(new TextDisplayProperties("The next invaders to defeat are..."));

        IEnumerable<SdrChannel> channels = dbContext.Set<SdrChannel>().Where(channel => channel.ServerId == server.Id).AsEnumerable().Where(channel => channel.Captor.TargetTotalPowerLevel > playerStrongestTpl);
        SortedSet<SdrChannel> sortedChannels = new(channels, new SdrChannelTplComparer());

        int entryNumber = 0;
        StringBuilder nextEnemies = new();
        foreach (SdrChannel channel in sortedChannels)
        {
            entryNumber++;
            nextEnemies.AppendLine($"{entryNumber}. **{channel.Captor.Name}**, in <#{channel.ChannelId}>");
            if (entryNumber >= 5)
            {
                break;
            }
        }

        components.Add(nextEnemies.Length <= 0
            ? new TextDisplayProperties("**None!** You've defeated all the invaders!!")
            : new TextDisplayProperties(nextEnemies.ToString()));

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
}
