using System;
using System.Linq;
using System.Threading.Tasks;
using NetCord.Services;
using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak;

public class InstanceOwnerPrecondition<TContext> : PreconditionAttribute<TContext> where TContext : IUserContext
{
    public override ValueTask<PreconditionResult> EnsureCanExecuteAsync(TContext context,
        IServiceProvider? serviceProvider)
    {
        if (serviceProvider?.GetService(typeof(OptionsManager)) is not OptionsManager options)
            return new ValueTask<PreconditionResult>(PreconditionResult.Fail("Internal failure."));
        if (!options.Get<IdentityOptions>().InstanceOwnerIds.Contains(context.User.Id))
            return new ValueTask<PreconditionResult>(PreconditionResult.Fail("You are not worthy."));

        return new ValueTask<PreconditionResult>(PreconditionResult.Success);
    }
}
