using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Serilog;
using Shardion.Terrabreak.Services.Discord;
using Shardion.Terrabreak.Services.Identity;

namespace Shardion.Terrabreak.Services.Interactions
{
    public class InteractionManager : ITerrabreakService
    {
        private readonly DiscordManager _discord;
        private readonly InteractionService _interactions;
        private readonly IdentityManager _identity;
        private readonly IServiceProvider _services;

        public InteractionManager(DiscordManager discord, IServiceProvider services, IdentityManager identity)
        {
            _discord = discord;
            _services = services;
            _identity = identity;
            _interactions = new(_discord.Client);
            _discord.Client.InteractionCreated += async (interaction) =>
            {
                SocketInteractionContext ctx = new(_discord.Client, interaction);
                try
                {
                    await _interactions.ExecuteCommandAsync(ctx, _services);
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                    EmbedBuilder errorEmbed = _identity.GetEmbedTemplate()
                        .WithTitle("Internal Error")
                        .WithDescription("An internal error occurred and your command couldn't be executed to completion. Ping <@208129127494975488> for assistance.");
                    await interaction.RespondAsync("", embed: errorEmbed.Build(), ephemeral: true);
                }
            };
            _discord.Client.Ready += async () =>
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
