using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shardion.Terrabreak.Features.Bags;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Timeout;

namespace Shardion.Terrabreak.Features.ManagementTools;

public static class DatabaseDumper
{

    public static async Task Dump(IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory)
    {
        Task dumpBagsTask = DumpSet<Bag>(await dbContextFactory.CreateDbContextAsync(), Path.Join(Entrypoint.ResolveConfigLocation("dump-bags")));
        Task dumpRemindersTask = DumpSet<Timeout>(await dbContextFactory.CreateDbContextAsync(), Path.Join(Entrypoint.ResolveConfigLocation("dump-timeouts")));
        await Task.WhenAll(dumpBagsTask, dumpRemindersTask);
    }

    public static async Task DumpSet<TSet>(TerrabreakDatabaseContext db, string outputFile) where TSet : class
    {
        IEnumerable<TSet> bags = db.Set<TSet>();
        FileStream outputStream = File.OpenWrite(outputFile);
        await JsonSerializer.SerializeAsync(outputStream, bags);
        outputStream.Close();
    }
}
