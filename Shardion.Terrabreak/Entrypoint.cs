using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Shardion.Terrabreak.Services.Options;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Shardion.Terrabreak.Services.Database;
using Microsoft.EntityFrameworkCore;

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

            foreach (Type serviceType in ReflectionHelper.GetAssignables<ITerrabreakFeature>())
            {
                builder.Services.AddSingleton(serviceType);
            }

            Action<DbContextOptionsBuilder> builderAction = ConfigureDb(builder.Configuration.GetRequiredSection("Database").Get<DatabaseOptions>()!);
            builder.Services.AddDbContext<TerrabreakDatabaseContext>(builderAction);
            builder.Services.AddDbContextFactory<TerrabreakDatabaseContext>(builderAction);

            using IHost host = builder.Build();

            List<Task> serviceTasks = [];
            foreach (Type serviceType in ReflectionHelper.GetAssignables<ITerrabreakService>())
            {
                if (host.Services.GetService(serviceType) is ITerrabreakService service)
                {
                    serviceTasks.Add(service.StartAsync());
                }
            }
            await Task.WhenAll(serviceTasks);

            List<Task> featureTasks = [];
            foreach (Type featureType in ReflectionHelper.GetAssignables<ITerrabreakFeature>())
            {
                if (host.Services.GetService(featureType) is ITerrabreakFeature feature)
                {
                    featureTasks.Add(feature.StartAsync());
                }
            }
            await Task.WhenAll(featureTasks);

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

        public static Action<DbContextOptionsBuilder> ConfigureDb(DatabaseOptions options)
        {
            return (DbContextOptionsBuilder builder) =>
            {
                builder.UseSqlite(options.ConnectionString);
#if DEBUG
                builder.EnableSensitiveDataLogging(true);
#endif
            };
        }
    }
}
