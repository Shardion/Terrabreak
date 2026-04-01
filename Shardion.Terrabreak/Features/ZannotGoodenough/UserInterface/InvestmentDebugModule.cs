using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Machina;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Nature;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Rodent;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Spirit;
using Shardion.Terrabreak.Features.ZannotGoodenough.Player;
using Shardion.Terrabreak.Features.ZannotGoodenough.Relics;
using Shardion.Terrabreak.Features.ZannotGoodenough.Relics.InvestorCore;
using Shardion.Terrabreak.Features.ZannotGoodenough.Relics.InvestorUltra;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Emoji;
using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Services.Menuing;
using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.UserInterface;

[InstanceOwnerPrecondition<ApplicationCommandContext>]
[SlashCommand("invest-debug", "Let's learn the basics of investing.",
    Contexts = [InteractionContextType.Guild], IntegrationTypes = [ApplicationIntegrationType.GuildInstall],
    DefaultGuildPermissions = Permissions.Administrator)]
public class InvestmentDebugModule(IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory, OptionsManager options, MenuManager menuManager, IdentityManager identity, EmojiManager emoji) : TerrabreakApplicationCommandModule(menuManager)
{
    [SubSlashCommand("battle", "Start a battle.")]
    public async Task StartBattle()
    {
        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        DiscordPlayer invokingPlayer = await dbContext.GetOrCreatePlayerAsync(options.Get<ZannotGoodenoughOptions>(), Context.User);

        BattleLoadout playerOne = new()
        {
            Player = invokingPlayer,
            Monster1 = new(new RazRat()),
            Monster2 = new(new LineBreaker()),
            Monster3 = new(new Nanite()),
            Relic1 = new(new BlurayBlastingBomb()),
            Relic2 = new(new FocusHarder()),
        };
        BattleLoadout playerTwo = new()
        {
            Player = new ComputerPlayer(),
            Monster1 = new(new RazRat()),
            Monster2 = new(new LineBreaker()),
            Monster3 = new(new Nanite()),
            Relic1 = new(new BlurayBlastingBomb()),
            Relic2 = new(new FocusHarder()),
        };
        Battle battle = new()
        {
            Player1 = playerOne,
            Player2 = playerTwo,
        };
        await ActivateMenuAsync(new InvestmentMenu(battle, identity, emoji, menuManager, null));
    }

    [SubSlashCommand("unlock-monster", "Unlock a monster for use.")]
    public async Task UnlockMonster(string name, User? user = null)
    {
        if (Registries.Monsters.Forward[name] is not IMonster<MonsterState> monster)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent($"Monster **`{name}`** does not exist!")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        User targetUser = user ?? Context.User;
        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        DiscordPlayer targetPlayer = await dbContext.GetOrCreatePlayerAsync(options.Get<ZannotGoodenoughOptions>(), targetUser, saveOnCreate: false);

        if (!dbContext.UnlockMonsterForPlayer(targetPlayer, monster))
        {
            await Task.WhenAll(
                RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                    .WithContent($"{targetPlayer.GetMention()} already has that monster!")
                    .WithFlags(MessageFlags.Ephemeral)
                )),
                // In case we made a new player with GetOrCreatePlayerAsync
                dbContext.SaveChangesAsync()
            );
            return;
        }
        Task saveTask = dbContext.SaveChangesAsync();
        Task sendResponseTask = RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            .WithContent($"Unlocked **{monster.Name}** for {targetPlayer.GetMention()}.")
        ));

        await Task.WhenAll(sendResponseTask, saveTask);
    }

    [SubSlashCommand("unlock-relic", "Unlock a relic for use.")]
    public async Task UnlockRelic(string name, User? user = null)
    {
        if (Registries.Relics.Forward[name] is not IRelic<RelicState> relic)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent($"Relic **`{name}`** does not exist!")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        User targetUser = user ?? Context.User;
        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        DiscordPlayer targetPlayer = await dbContext.GetOrCreatePlayerAsync(options.Get<ZannotGoodenoughOptions>(), targetUser, saveOnCreate: false);

        if (!dbContext.UnlockRelicForPlayer(targetPlayer, relic))
        {
            await Task.WhenAll(
                RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                    .WithContent($"{targetPlayer.GetMention()} already has that relic!")
                    .WithFlags(MessageFlags.Ephemeral)
                )),
                // In case we made a new player with GetOrCreatePlayerAsync
                dbContext.SaveChangesAsync()
            );
            return;
        }
        Task saveTask = dbContext.SaveChangesAsync();
        Task sendResponseTask = RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            .WithContent($"Unlocked **{relic.Name}** for {targetPlayer.GetMention()}.")
        ));

        await Task.WhenAll(sendResponseTask, saveTask);
    }

    [SubSlashCommand("clear", "Clear a profile.")]
    public async Task Clear(User user)
    {
        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        if (dbContext.GetPlayer(options.Get<ZannotGoodenoughOptions>(), user) is not DiscordPlayer player)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent($"<@{user.Id}> doesn't have player data associated!")
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        dbContext.Set<DiscordPlayer>().Remove(player);
        Task saveTask = dbContext.SaveChangesAsync();
        Task sendResponseTask = RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            .WithContent($"Cleared <@{user.Id}>.")
            .WithFlags(MessageFlags.Ephemeral)
        ));

        await Task.WhenAll(sendResponseTask, saveTask);
    }
}
