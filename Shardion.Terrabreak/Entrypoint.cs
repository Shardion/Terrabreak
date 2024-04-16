using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Discord;
using Shardion.Terrabreak.Services.Options;
using Shardion.Terrabreak.Services.Discord;
using Shardion.Terrabreak.Services.Interactions;

namespace Shardion.Terrabreak
{
    public class Entrypoint
    {
        public static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            builder.Configuration.Sources.Clear();
            builder.Configuration
                .AddJsonFile(ResolveConfigLocation("config"), optional: true, reloadOnChange: true)
                .AddJsonFile(ResolveConfigLocation("token"), optional: false, reloadOnChange: false);

            foreach (IStaticOptions staticOptions in ReflectionHelper.ConstructParameterlessAssignables<IStaticOptions>())
            {
                builder.Configuration.GetSection(staticOptions.SectionName).Bind(staticOptions);
#pragma warning disable IDE0001 // Has different behavior with the type argument
                builder.Services.AddSingleton<IStaticOptions>(staticOptions);
#pragma warning restore IDE0001
                builder.Services.AddSingleton(staticOptions.GetType(), staticOptions);
            }

            foreach (IDynamicOptions dynamicOptions in ReflectionHelper.ConstructParameterlessAssignables<IDynamicOptions>())
            {
                builder.Configuration.GetSection(dynamicOptions.SectionName).Bind(dynamicOptions);
#pragma warning disable IDE0001 // Has different behavior with the type argument
                builder.Services.AddSingleton<IDynamicOptions>(dynamicOptions);
#pragma warning restore IDE0001
                builder.Services.AddSingleton(dynamicOptions.GetType(), dynamicOptions);
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            builder.Services.AddSerilog(null, true, null);

            foreach (Type serviceType in ReflectionHelper.GetAssignables<ITerrabreakService>())
            {
                builder.Services.AddSingleton(serviceType);
            }

            using IHost host = builder.Build();

            DiscordManager discord = host.Services.GetRequiredService<DiscordManager>();
            InteractionManager interactions = host.Services.GetRequiredService<InteractionManager>();
            discord.Client.Log += LogAsync;

            Task[] tasks =
            [
                discord.StartAsync(),
                interactions.StartAsync(),
            ];
            await Task.WhenAll(tasks);
            await Task.Delay(Timeout.Infinite);
        }

        public static string ResolveConfigLocation(string filename, string extension = ".json")
        {
            foreach (string arg in Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith($"--{filename}="))
                {
                    return Path.Join(arg.Replace($"--{filename}=", ""), $"{filename}{extension}");
                }
            }
            if (Environment.GetEnvironmentVariable("TERRABREAK_CONFIG_HOME") is string configDir)
            {
                return Path.Join(configDir, $"{filename}{extension}");
            }
            if (Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") is string configHome)
            {
                return Path.Join(configHome, "terrabreak", $"{filename}{extension}");
            }
            if (Environment.GetEnvironmentVariable("HOME") is string home && Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                return Path.Join(home, ".config", "terrabreak", $"{filename}{extension}");
            }
            return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "terrabreak", $"{filename}{extension}");
        }

        private static async Task LogAsync(LogMessage message)
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
    }
}
