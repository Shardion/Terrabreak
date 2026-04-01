using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Services.ApplicationCommands;
using Shardion.Terrabreak.Features.ZannotGoodenough.Player;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Emoji;
using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Services.Menuing;
using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.UserInterface;

public class LoadoutModule(IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory, OptionsManager options, MenuManager menu, IdentityManager identity, EmojiManager emoji)
    : TerrabreakApplicationCommandModule(menu)
{
    [SlashCommand("loadout", "View and edit your loadout.", Contexts = [InteractionContextType.BotDMChannel, InteractionContextType.DMChannel, InteractionContextType.Guild])]
    public async Task ViewLoadouts()
    {
        TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
        DiscordPlayer invokingPlayer = await dbContext.GetOrCreatePlayerAsync(options.Get<ZannotGoodenoughOptions>(), Context.User);
        await ActivateMenuAsync(new LoadoutMenu(dbContextFactory, identity, emoji, menu, invokingPlayer));
    }
}
