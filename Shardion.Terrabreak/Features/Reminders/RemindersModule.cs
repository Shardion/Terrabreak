using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Quartz;
using Quartz.Impl.Matchers;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.Reminders;

public class RemindersModule(ISchedulerFactory schedulerFactory, MenuManager menuManager)
    : TerrabreakApplicationCommandModule(menuManager)
{
    [SlashCommand("reminders", "List and delete reminders.",
        Contexts =
        [
            InteractionContextType.BotDMChannel, InteractionContextType.DMChannel, InteractionContextType.Guild
        ])]
    public async Task Reminders()
    {
        await ActivateMenuAsync(new RemindersMenu(schedulerFactory, Context.Interaction.User.Id)
            {
                AllowedUsers = new HashSet<ulong>([Context.Interaction.User.Id])
            }
        );
    }
}
