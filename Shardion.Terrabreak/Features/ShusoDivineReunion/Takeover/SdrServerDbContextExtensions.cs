using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Takeover;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Takeover;

public static class SdrServerDbContextExtensions
{
    public static SdrServer CreateServer(this DbContext context, ulong serverId)
    {
        SdrServer server = new()
        {
            ServerId = serverId,
        };
        context.Add(server);
        return server;
    }

    public static SdrServer? GetServer(this DbContext context, ulong serverId)
    {
        return context.Set<SdrServer>().FirstOrDefault(server => server.ServerId == serverId);
    }

    public static async Task<SdrServer> GetOrCreateServerAsync(this DbContext context, ulong serverId)
    {
        SdrServer? existingServer = context.Set<SdrServer>().FirstOrDefault(server => server.ServerId == serverId);
        if (existingServer is not null)
        {
            return existingServer;
        }

        SdrServer newServer = context.CreateServer(serverId);
        context.Add(newServer);
        await context.SaveChangesAsync();
        return newServer;
    }
}
