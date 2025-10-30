using System.Linq;
using Microsoft.EntityFrameworkCore;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Takeover;

public static class SdrChannelDbContextExtensions
{
    public static SdrChannel? GetChannel(this DbContext context, ulong channelId)
    {
        return context.Set<SdrChannel>().FirstOrDefault(channel => channel.ChannelId == channelId);
    }
}
