using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Serilog;
using Shardion.Terrabreak.Services.Discord;
using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Services.Interactions
{
    public class InteractionManager : ITerrabreakService
    {
        private readonly DiscordManager _discordManager;
        private readonly InteractionService _interactions;
        private readonly IdentityManager _identityManager;
        private readonly OptionsManager _optionsManager;
        private readonly IServiceProvider _services;

        public InteractionManager(DiscordManager discordManager, IServiceProvider services, IdentityManager identityManager, OptionsManager optionsManager)
        {
            _discordManager = discordManager;
            _services = services;
            _identityManager = identityManager;
            _optionsManager = optionsManager;
            _interactions = new(_discordManager.Client);
            _discordManager.Client.InteractionCreated += async (interaction) =>
            {
                SocketInteractionContext ctx = new(_discordManager.Client, interaction);
                try
                {
                    await _interactions.ExecuteCommandAsync(ctx, _services);
                }
                catch (Exception)
                {
                    string primaryDevSection = _optionsManager.Get<IdentityOptions>().PrimaryDeveloperID is ulong primaryDevId ? $"Please ping <@{primaryDevId}> for assistance." : "";
                    EmbedBuilder errorEmbed = _identityManager.GetEmbedTemplate()
                        .WithTitle("Internal Error")
                        .WithDescription($"An internal error occurred and your command couldn't be executed to completion. {primaryDevSection}");
                    await interaction.RespondAsync("", embed: errorEmbed.Build(), ephemeral: true);
                }
            };
            _discordManager.Client.Ready += async () =>
            {
                await _interactions.AddModulesAsync(typeof(Entrypoint).Assembly, _services);
                await _interactions.RegisterCommandsGloballyAsync();
            };
        }

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }
    }
}
