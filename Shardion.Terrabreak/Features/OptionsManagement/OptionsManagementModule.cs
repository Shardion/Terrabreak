using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Shardion.Terrabreak.Services.Identity;

namespace Shardion.Terrabreak.Features.OptionsManagement
{
    [CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
    [IntegrationType(ApplicationIntegrationType.UserInstall, ApplicationIntegrationType.GuildInstall)]
    public class OptionsManagementModule : InteractionModuleBase
    {
        private readonly IdentityManager _identity;

        public OptionsManagementModule(IdentityManager identity)
        {
            _identity = identity;
        }

        [SlashCommand("options", "Views and modifies options.")]
        public async Task Options()
        {
            await Context.Interaction.RespondAsync(
            "Greetings from shardion! Because of the immense complexity of implementing " +
            "real menuing, and that it was blocking me from adding Automute, this command " +
            "has been hardcoded to handle exclusively server-wide settings for Automute. " +
            "This was done as a measure to reach feature parity with Achromatic sooner.\n\n" +
            "- To enable automute, use `/options automute-enabled`.\n" +
            "- To change the reaction emoji, use `/options automute-emoji`.\n" +
            "- To change the amount of reactions required, use `/options automute-reactions`.\n\n" +
            "Be warned, the emoji option does little-to-no validation, " +
            "meaning you can add emojis that don't exist and effectively disable Automute. "
            );
        }
    }
}
