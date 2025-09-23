using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Interactions;

namespace Shardion.Terrabreak.Features.ManagementTools
{
    [CommandContextType(InteractionContextType.Guild)]
    [IntegrationType(ApplicationIntegrationType.GuildInstall)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [InstanceOwnerPrecondition]
    public class ManagementToolsModule(IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory) : InteractionModuleBase
    {
        [SlashCommand("impersonate", "Impersonate the bot.")]
        public async Task Impersonate(
            [Summary(description: "The message to make the bot send.")] string message,
            [Summary(description: "The channel to send the message in.")] ITextChannel channel
            )
        {
            await channel.SendMessageAsync(message);
            await RespondAsync("Message sent.", ephemeral: true);
        }

        [SlashCommand("dump", "Dump the database to a file.")]
        public async Task Dump()
        {
            Task deferTask = DeferAsync(ephemeral: true);
            await DatabaseDumper.Dump(dbContextFactory);
            await deferTask;
            await ModifyOriginalResponseAsync(message =>
            {
                message.Content = "Database dumped.";
            });
        }
    }
}
