using System;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Serilog;
using Serilog.Events;
using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Services.Discord
{
    public class DiscordManager : ITerrabreakService, IDisposable, IAsyncDisposable
    {
        public DiscordSocketClient Client { get => _client ?? throw new ObjectDisposedException(typeof(DiscordSocketClient).FullName ?? typeof(DiscordSocketClient).Name); }
        private DiscordSocketClient? _client;

        public DiscordRestClient RestClient { get => _restClient ?? throw new ObjectDisposedException(typeof(DiscordRestClient).FullName ?? typeof(DiscordRestClient).Name); }
        private DiscordRestClient? _restClient;

        private readonly DiscordManagerOptions _discordOptions;
        private readonly OptionsManager _options;

        private System.Timers.Timer? _statusTimer;
        private string? _lastSplash;

        public DiscordManager(OptionsManager options, DiscordManagerOptions discordOptions)
        {
            _discordOptions = discordOptions;
            _options = options;

            DiscordSocketConfig socketConfig = new()
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.DirectMessages | GatewayIntents.GuildMessageReactions,
#if DEBUG
                LogLevel = LogSeverity.Verbose,
#endif
            };

            DiscordRestConfig restConfig = new()
            {
#if DEBUG
                LogLevel = LogSeverity.Verbose,
#endif
            };

            _restClient = new(restConfig);
            _restClient.Log += LogAsync;

            _client = new(socketConfig);
            Client.Log += LogAsync;
            Client.Ready += async () =>
            {
                await RotateStatus(false);
            };
        }

        public async Task StartAsync()
        {
            await RestClient.LoginAsync(TokenType.Bot, _discordOptions.Token);
            await Client.LoginAsync(TokenType.Bot, _discordOptions.Token);
            await Client.StartAsync();
        }

        private async Task RotateStatus(bool timerControlled)
        {
            if (timerControlled || _statusTimer is null)
            {
                if (_options.Get<IdentityOptions>() is IdentityOptions identity && identity.Splashes is string[] splashes)
                {
                    string splash = splashes[Random.Shared.Next(splashes.Length)];
                    if (splash != _lastSplash)
                    {
                        _lastSplash = splash;
                        await Client.SetCustomStatusAsync(splash);
                    }
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

        public static async Task LogAsync(LogMessage message)
        {
            var severity = message.Severity switch
            {
                LogSeverity.Critical => LogEventLevel.Fatal,
                LogSeverity.Error => LogEventLevel.Error,
                LogSeverity.Warning => LogEventLevel.Warning,
                LogSeverity.Info => LogEventLevel.Information,
                LogSeverity.Verbose => LogEventLevel.Verbose,
                LogSeverity.Debug => LogEventLevel.Debug,
                _ => LogEventLevel.Information
            };
            Log.Write(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);
            await Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(disposingManagedObjects: true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);

            Dispose(disposingManagedObjects: false);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposingManagedObjects)
        {
            if (disposingManagedObjects)
            {
                _client?.Dispose();
                _client = null;
                _restClient?.Dispose();
                _restClient = null;
            }
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (_client is not null)
            {
                await _client.DisposeAsync().ConfigureAwait(false);
            }

            _client = null;

            if (_restClient is not null)
            {
                await _restClient.DisposeAsync().ConfigureAwait(false);
            }

            _restClient = null;
        }
    }
}
