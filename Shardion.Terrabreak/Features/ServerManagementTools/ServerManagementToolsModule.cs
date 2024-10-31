using System.Threading.Tasks;

using Discord;
using Discord.Interactions;

using Shardion.Terrabreak.Services.Interactions;

namespace Shardion.Terrabreak.Features.ServerManagementTools
{
    [CommandContextType(InteractionContextType.Guild)]
    [IntegrationType(ApplicationIntegrationType.GuildInstall)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [InstanceOwnerPrecondition]
    public class ServerManagementToolsModule : InteractionModuleBase
    {
        [SlashCommand("impersonate", "Impersonate the bot.")]
        public async Task SelfPurge(
            [Summary(description: "The message to make the bot send.")] string message,
            [Summary(description: "The channel to send the message in.")] ITextChannel channel
            )
        {
            await channel.SendMessageAsync(message);
            await RespondAsync("Message sent.", ephemeral: true);
        }
    }
}
