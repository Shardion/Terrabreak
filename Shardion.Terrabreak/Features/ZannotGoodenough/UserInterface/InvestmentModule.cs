using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Services.ApplicationCommands;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Player;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Emoji;
using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Services.Menuing;
using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.UserInterface;

public class InvestmentModule(IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory, OptionsManager options, MenuManager menu, IdentityManager identity, EmojiManager emoji)
    : TerrabreakApplicationCommandModule(menu)
{
    [SlashCommand("invest", "Invest now!", Contexts = [InteractionContextType.BotDMChannel, InteractionContextType.DMChannel, InteractionContextType.Guild])]
    public async Task Invest()
    {
        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        DiscordPlayer invokingPlayer = await dbContext.GetOrCreatePlayerAsync(options.Get<ZannotGoodenoughOptions>(), Context.User);
        await ActivateMenuAsync(new StadiumMenu(dbContextFactory, identity, emoji, menu, invokingPlayer));
    }

    [SlashCommand("duel", "Invest in eachother's suffering!", Contexts = [InteractionContextType.BotDMChannel, InteractionContextType.DMChannel, InteractionContextType.Guild])]
    public async Task Duel(User opponent)
    {
        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        DiscordPlayer invokingPlayer = await dbContext.GetOrCreatePlayerAsync(options.Get<ZannotGoodenoughOptions>(), Context.User);
        DiscordPlayer opponentPlayer = await dbContext.GetOrCreatePlayerAsync(options.Get<ZannotGoodenoughOptions>(), opponent);

        Battle battle = new()
        {
            Player1 = BattleLoadout.FromLoadout(invokingPlayer.EquippedLoadout, invokingPlayer),
            Player2 = BattleLoadout.FromLoadout(opponentPlayer.EquippedLoadout, opponentPlayer),
        };

        await ActivateMenuAsync(new DuelMenu(identity, emoji, menu, invokingPlayer, opponentPlayer));
    }
}
