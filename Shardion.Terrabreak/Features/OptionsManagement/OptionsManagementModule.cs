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
            EmbedBuilder embed = _identity.GetEmbedTemplate();
            embed.Color = new Color(0xED4245);
            embed.Title = "Dummy category";
            embed.Description = "__Page 1/1__\n1. Filler\n2. Filler\n3. Filler";
            embed.Footer = new()
            {
                Text = "Currently viewing Global options",
                IconUrl = "https://anomaly.tail354c3.ts.net/assets/bulb.png"
            };

            SelectMenuBuilder dropdown = new()
            {
                CustomId = "category"
            };
            dropdown.AddOption("Dummy category", "Hi!");
            dropdown.AddOption("Another dummy category", "Lea!");
            dropdown.AddOption("Yet another dummy category", "Bye!");

            ActionRowBuilder dropdownRow = new();
            dropdownRow.AddComponent(dropdown.Build());

            ButtonBuilder modifyButton = new()
            {
                Label = "Modify",
                CustomId = "modify",
                Style = ButtonStyle.Secondary,
            };

            ButtonBuilder previousButton = new()
            {
                Emote = new Emoji("⬆"),
                CustomId = "previous",
                IsDisabled = true,
                Style = ButtonStyle.Secondary,
            };

            ButtonBuilder nextButton = new()
            {
                Emote = new Emoji("⬇"),
                CustomId = "next",
                IsDisabled = true,
                Style = ButtonStyle.Secondary,
            };

            ActionRowBuilder navigationRow = new();
            navigationRow.AddComponent(modifyButton.Build());
            navigationRow.AddComponent(previousButton.Build());
            navigationRow.AddComponent(nextButton.Build());

            ButtonBuilder userButton = new()
            {
                Label = "User",
                CustomId = "user",
                Style = ButtonStyle.Success,
            };

            ButtonBuilder serverButton = new()
            {
                Label = "Server",
                CustomId = "server",
                Style = ButtonStyle.Primary,
            };

            ButtonBuilder globalButton = new()
            {
                Label = "Global",
                CustomId = "global",
                Style = ButtonStyle.Danger,
            };

            ActionRowBuilder contextRow = new();
            contextRow.AddComponent(userButton.Build());
            contextRow.AddComponent(serverButton.Build());
            contextRow.AddComponent(globalButton.Build());

            ComponentBuilder components = new();
            components.AddRow(dropdownRow);
            components.AddRow(navigationRow);
            components.AddRow(contextRow);

            await Context.Interaction.RespondAsync("", embed: embed.Build(), components: components.Build());
        }
    }
}
