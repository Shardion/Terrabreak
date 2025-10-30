using System;
using System.Threading.Tasks;
using NetCord.Services;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Takeover;

public class TakeoverStartedPrecondition<TContext> : PreconditionAttribute<TContext> where TContext : IUserContext
{
    public override ValueTask<PreconditionResult> EnsureCanExecuteAsync(TContext context,
        IServiceProvider? serviceProvider)
    {
        if (serviceProvider?.GetService(typeof(TakeoverManager)) is not TakeoverManager takeoverManager)
        {
            return new(PreconditionResult.Fail("Internal failure."));
        }

        if (takeoverManager.TakeoverTimestamp > DateTimeOffset.UtcNow)
        {
            return new(PreconditionResult.Fail("You are not worthy."));
        }

        return new(PreconditionResult.Success);
    }
}

