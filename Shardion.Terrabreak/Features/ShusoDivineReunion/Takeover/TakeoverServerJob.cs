using System;
using System.Threading.Tasks;
using NetCord;
using NetCord.Gateway;
using Quartz;
using Serilog;
using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Takeover;

public class TakeoverServerJob(GatewayClient gateway, OptionsManager optionsManager, TakeoverManager takeoverManager) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        ShusoDivineReunionOptions sdrOptions = optionsManager.Get<ShusoDivineReunionOptions>();
        if (sdrOptions.TakeoverServerId is ulong takeoverServerId)
        {
            await takeoverManager.TakeoverServerAsync(takeoverServerId);
        }
    }
}
