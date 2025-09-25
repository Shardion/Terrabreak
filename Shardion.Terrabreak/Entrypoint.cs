using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Shardion.Terrabreak.Services.Options;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using Shardion.Terrabreak.Services.Database;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;
using Quartz;
using Shardion.Terrabreak.Features.Bags;
using Shardion.Terrabreak.Utilities;

namespace Shardion.Terrabreak;

public class Entrypoint
{
    public static HostApplicationBuilder CreateHostApplicationBuilder(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Configuration.Sources.Clear();
        builder.Configuration
            .AddJsonFile(Path.Join(GetConfigurationDirectoryPath(), "config.json"), true, true)
            .AddJsonFile(Path.Join(GetConfigurationDirectoryPath(), "token.json"), false, false);

        foreach (IStaticOptions staticOptions in ReflectionUtil.ConstructParameterlessAssignables<IStaticOptions>())
        {
            builder.Configuration.GetSection(staticOptions.SectionName).Bind(staticOptions);
#pragma warning disable IDE0001 // Has different behavior with the type argument
            // ReSharper disable once RedundantTypeArgumentsOfMethod
            builder.Services.AddSingleton<IStaticOptions>(staticOptions);
#pragma warning restore IDE0001
            builder.Services.AddSingleton(staticOptions.GetType(), staticOptions);
        }

        foreach (IDynamicOptions dynamicOptions in ReflectionUtil.ConstructParameterlessAssignables<IDynamicOptions>())
        {
            builder.Configuration.GetSection(dynamicOptions.SectionName).Bind(dynamicOptions);
#pragma warning disable IDE0001 // Has different behavior with the type argument
            // ReSharper disable once RedundantTypeArgumentsOfMethod
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
        builder.Services.AddQuartz(options =>
        {
            options.UsePersistentStore(x =>
            {
                x.UseMicrosoftSQLite(builder.Configuration.GetRequiredSection("Database").Get<DatabaseOptions>()!
                    .ConnectionString);
                x.UseSystemTextJsonSerializer();
            });
        });
        builder.Services.AddQuartzHostedService(options => { options.WaitForJobsToComplete = true; });
        builder.Services.AddDiscordGateway(options =>
        {
            options.Intents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.DirectMessages |
                              GatewayIntents.GuildMessageReactions;
            options.Token = builder.Configuration.GetSection("Discord")["Token"];
        });
        builder.Services.AddGatewayHandlers(typeof(Entrypoint).Assembly);
        builder.Services.AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>();
        builder.Services.AddApplicationCommands();

        foreach (Type serviceType in ReflectionUtil.GetAssignables<ITerrabreakService>())
            builder.Services.AddSingleton(serviceType);

        foreach (Type serviceType in ReflectionUtil.GetAssignables<ITerrabreakFeature>())
            builder.Services.AddSingleton(serviceType);

        Action<DbContextOptionsBuilder> builderAction =
            ConfigureDb(builder.Configuration.GetRequiredSection("Database").Get<DatabaseOptions>()!);
        builder.Services.AddDbContext<TerrabreakDatabaseContext>(builderAction);
        builder.Services.AddDbContextFactory<TerrabreakDatabaseContext>(builderAction);

        return builder;
    }

    public static async Task Main(string[] args)
    {
        using IHost host = CreateHostApplicationBuilder(args).Build();

        List<Task> serviceTasks = [];
        foreach (Type serviceType in ReflectionUtil.GetAssignables<ITerrabreakService>())
            if (host.Services.GetService(serviceType) is ITerrabreakService service)
                serviceTasks.Add(service.StartAsync());

        await Task.WhenAll(serviceTasks);

        List<Task> featureTasks = [];
        foreach (Type featureType in ReflectionUtil.GetAssignables<ITerrabreakFeature>())
            if (host.Services.GetService(featureType) is ITerrabreakFeature feature)
                featureTasks.Add(feature.StartAsync());

        await Task.WhenAll(featureTasks);
        host.AddModules(typeof(Entrypoint).Assembly);

        await host.RunAsync();
    }

    public static string GetConfigurationDirectoryPath()
    {
        if (Environment.GetEnvironmentVariable("TERRABREAK_CONFIG_HOME") is string configDir)
            return Path.Join(configDir);
        if (Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") is string configHome)
            return Path.Join(configHome, "terrabreak");
        if (Environment.GetEnvironmentVariable("HOME") is string home &&
            Environment.OSVersion.Platform != PlatformID.Win32NT) return Path.Join(home, ".config", "terrabreak");
        return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "terrabreak");
    }

    public static Action<DbContextOptionsBuilder> ConfigureDb(DatabaseOptions options)
    {
        return builder =>
        {
            builder.UseSqlite(options.ConnectionString);
#if DEBUG
            builder.EnableSensitiveDataLogging();
#endif
        };
    }
}
