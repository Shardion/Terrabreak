using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shardion.Terrabreak.Utilities;

namespace Shardion.Terrabreak.Services.Options;

public class OptionsManager : ITerrabreakService
{
    private readonly IServiceProvider _provider;

    public OptionsManager(IServiceProvider provider)
    {
        _provider = provider;
    }

    public TOptions Get<TOptions>(ulong? userId = null, ulong? serverId = null) where TOptions : class, IDynamicOptions
    {
        TOptions? options = GetInternal<TOptions>();
        if (options is null)
            throw new InvalidOperationException($"There is no options of type {typeof(TOptions).Name}.");
        else if (Accessible(options.Permissions, userId, serverId))
            return options;
        else
            throw new NoAccessException();
    }

    public IReadOnlyCollection<TOptions> GetMany<TOptions>(ulong? userId = null, ulong? serverId = null)
        where TOptions : class, IDynamicOptions
    {
        List<TOptions> gotOptions = [];
        foreach (Type type in ReflectionUtil.GetParameterlessConstructibleAssignables<TOptions>())
            if (GetInternal(type) is TOptions options)
                if (Accessible(options.Permissions, userId, serverId))
                    gotOptions.Add(options);

        return gotOptions.AsReadOnly();
    }

    private static bool Accessible(OptionsPermissions permissions, ulong? userId = null, ulong? serverId = null)
    {
        if (userId is not null)
            return permissions.IsAccessible(OptionsAccessor.User, OptionsAccessibility.ReadWrite);
        else if (serverId is not null)
            return permissions.IsAccessible(OptionsAccessor.Server, OptionsAccessibility.ReadWrite);
        else
            return permissions.IsAccessible(OptionsAccessor.Bot, OptionsAccessibility.ReadWrite);
    }

    private TOptions? GetInternal<TOptions>() where TOptions : class, IDynamicOptions
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
