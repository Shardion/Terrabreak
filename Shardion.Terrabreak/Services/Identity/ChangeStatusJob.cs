using System;
using System.Threading.Tasks;
using NetCord;
using NetCord.Gateway;
using Quartz;
using Serilog;

namespace Shardion.Terrabreak.Services.Identity;

public class ChangeStatusJob(GatewayClient gateway, IdentityManager identity) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        if (identity.Options.Splashes is null)
        {
            if (identity.LastSplash is not null)
            {
                identity.LastSplash = null;
                await gateway.UpdatePresenceAsync(new PresenceProperties(UserStatusType.Online));
            }
        }
        else
        {
            string randomSplash = identity.Options.Splashes[Random.Shared.Next(identity.Options.Splashes.Length)];
            if (randomSplash != identity.LastSplash)
            {
                PresenceProperties presence = new PresenceProperties(UserStatusType.Online)
                    .WithActivities([
                        new UserActivityProperties("Splash", UserActivityType.Custom)
                        {
                            State = randomSplash
                        }
                    ]);
                await gateway.UpdatePresenceAsync(presence);
            }
        }
    }
}
