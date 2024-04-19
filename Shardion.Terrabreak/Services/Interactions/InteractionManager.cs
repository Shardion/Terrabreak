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

            InteractionServiceConfig config = new()
            {
                UseCompiledLambda = true,
#if DEBUG
                LogLevel = LogSeverity.Verbose,
#endif
            };

            _interactions = new(_discordManager.Client, config);

            _discordManager.Client.InteractionCreated += async (interaction) =>
            {
                SocketInteractionContext ctx = new(_discordManager.Client, interaction);
                await _interactions.ExecuteCommandAsync(ctx, _services);
            };

            _interactions.InteractionExecuted += async (command, context, result) =>
            {
                if (!result.IsSuccess && result.Error is not null)
                {
                    string primaryDevSection;
                    if (_optionsManager.Get<IdentityOptions>().PrimaryDeveloperID is ulong primaryDevId)
                    {
                        primaryDevSection = $" Please ping <@{primaryDevId}> for assistance.";
                    }
                    else
                    {
                        primaryDevSection = "";
                    }

                    EmbedBuilder errorEmbed = _identityManager.GetEmbedTemplate()
                        .WithTitle("Internal Error")
                        .WithDescription($"An internal error occurred and your command couldn't be executed to completion.{primaryDevSection}")
                        .AddField("Error", $"**`{result.Error}`**: {result.ErrorReason}");
                    await context.Interaction.RespondAsync("", embed: errorEmbed.Build(), ephemeral: true);
                }
            };

            _interactions.Log += DiscordManager.LogAsync;

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
