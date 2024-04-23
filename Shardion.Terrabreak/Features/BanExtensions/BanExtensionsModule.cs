using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.Net;

namespace Shardion.Terrabreak.Features.BanExtensions
{
    [CommandContextType(InteractionContextType.Guild)]
    [IntegrationType(ApplicationIntegrationType.GuildInstall)]
    [DefaultMemberPermissions(GuildPermission.BanMembers)]
    public class BanExtensionsModule : InteractionModuleBase
    {
        [SlashCommand("ban-id", "Bans a user by their User ID.")]
        public async Task BanById(
            [Summary(description: "The ID of the user to ban.")] string userId,
            [Summary(description: "The number of days of messages to prune, if applicable. Must not be greater than 7.")] int pruneDays = 0,
            [Summary(description: "The reason as seen in the audit log and ban list, if applicable.")] string? reason = null
        )
        {
            if (!ulong.TryParse(userId, NumberStyles.None, CultureInfo.InvariantCulture, out ulong parsedUserId))
            {
                await RespondAsync("Specified user ID is not a valid number.", ephemeral: true);
            }
            else
            {
                try
                {
                    await Context.Guild.AddBanAsync(parsedUserId, pruneDays, reason);
                    await RespondAsync($"Banned user with ID **`{parsedUserId.ToString(CultureInfo.InvariantCulture)}`**.", ephemeral: true);
                }
                catch (HttpException)
                {
                    await RespondAsync($"Failed to ban user with ID {parsedUserId}. Does that user exist, and does the bot have permission to ban them?");
                }
            }
        }
    }
}
