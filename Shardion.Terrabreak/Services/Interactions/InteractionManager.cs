using System;
using System.Threading.Tasks;
using Discord.Interactions;
using Shardion.Terrabreak.Services.Discord;

namespace Shardion.Terrabreak.Services.Interactions
{
    public class InteractionManager : ITerrabreakService
    {
        private readonly DiscordManager _discord;
        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;

        public InteractionManager(DiscordManager discord, IServiceProvider services)
        {
            _discord = discord;
            _services = services;
            _interactions = new(_discord.Client);
            _discord.Client.InteractionCreated += async (interaction) =>
            {
                SocketInteractionContext ctx = new(_discord.Client, interaction);
                await _interactions.ExecuteCommandAsync(ctx, _services);
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
