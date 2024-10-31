using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Interactions;

using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Services.Interactions
{
    public class InstanceOwnerPrecondition : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (services.GetService(typeof(OptionsManager)) is not OptionsManager options)
            {
                return Task.FromResult(PreconditionResult.FromError(new NullReferenceException("options")));
            }

            if (options.Get<IdentityOptions>().InstanceOwnerIds.Contains(context.User.Id))
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }

            return Task.FromResult(PreconditionResult.FromError("User is not an instance owner."));
        }
    }
}
