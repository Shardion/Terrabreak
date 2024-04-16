using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Services.Discord
{
    public class DiscordManager : ITerrabreakService
    {
        public DiscordSocketClient Client { get; }

        private readonly DiscordManagerOptions _discordOptions;
        private readonly OptionsManager _options;
        private readonly IServiceProvider _services;

        private System.Timers.Timer? _statusTimer;

        public DiscordManager(OptionsManager options, DiscordManagerOptions discordOptions, IServiceProvider services)
        {
            _discordOptions = discordOptions;
            _options = options;
            _services = services;

            DiscordSocketConfig config = new()
            {
                GatewayIntents = GatewayIntents.GuildPresences | GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.DirectMessages
            };

            Client = new(config);
            Client.Ready += async () =>
            {
                await RotateStatus(false);
            };
        }

        public async Task StartAsync()
        {
            await Client.LoginAsync(TokenType.Bot, _discordOptions.Token);
            await Client.StartAsync();
        }

        private async Task RotateStatus(bool timerControlled)
        {
            if (timerControlled || _statusTimer is null)
            {
                if (_options.Get<IdentityOptions>() is IdentityOptions identity)
                {
                    string splash = identity.Splashes[Random.Shared.Next(identity.Splashes.Length)];
                    await Client.SetCustomStatusAsync(splash);
                }
            }

            if (_statusTimer is null)
            {
                _statusTimer = new()
                {
                    AutoReset = true,
                    Interval = TimeSpan.FromMinutes(5).TotalMilliseconds,
                    Enabled = false,
                };
                _statusTimer.Elapsed += async (_, _) => await RotateStatus(true);
                _statusTimer.Start();
            }
        }
    }
}
