using Microsoft.Extensions.DependencyInjection;

namespace Shardion.Terrabreak.Services.Options
{
    public class OptionsManager : ITerrabreakService
    {
        private readonly IServiceProvider _provider;

        public OptionsManager(IServiceProvider provider)
        {
            _provider = provider;
        }

        public TOptions? Get<TOptions>(ulong? userId = null, ulong? serverId = null) where TOptions : class, IDynamicOptions, new()
        {
            TOptions options = GetInternal<TOptions>() ?? new();
            if (Accessible(options.Permissions, userId, serverId))
            {
                return options;
            }
            return null;
        }

        public IReadOnlyCollection<TOptions> GetMany<TOptions>(ulong? userId = null, ulong? serverId = null) where TOptions : IDynamicOptions
        {
            List<TOptions> gotOptions = [];
            foreach (Type type in ReflectionHelper.GetParameterlessConstructibleAssignables<TOptions>())
            {
                if (GetInternal(type) is TOptions options)
                {
                    if (Accessible(options.Permissions, userId, serverId))
                    {
                        gotOptions.Add(options);
                    }
                }
            }
            return gotOptions.AsReadOnly();
        }

        private static bool Accessible(OptionsPermissions permissions, ulong? userId = null, ulong? serverId = null)
        {
            if (userId is not null)
            {
                return permissions.AccessibleTo(OptionsAccessor.User, OptionsAccessibility.Read);
            }
            else if (serverId is not null)
            {
                return permissions.AccessibleTo(OptionsAccessor.Server, OptionsAccessibility.Read);
            }
            else
            {
                return permissions.AccessibleTo(OptionsAccessor.Bot, OptionsAccessibility.Read);
            }
        }

        private TOptions? GetInternal<TOptions>() where TOptions : IDynamicOptions, new()
        {
            return _provider.GetService<TOptions>();
        }

        private IDynamicOptions? GetInternal(Type type)
        {
            return _provider.GetService(type) as IDynamicOptions;
        }

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }
    }
}
