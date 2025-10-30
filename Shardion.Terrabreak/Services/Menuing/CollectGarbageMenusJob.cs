using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using Serilog;

namespace Shardion.Terrabreak.Services.Menuing;

public class CollectGarbageMenusJob(MenuManager menuManager) : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        // ToList() intentional, to copy all menus out to a seperate list.
        // Single-threaded, and not ideal, but nobody will ever use this bot anyways, right...?
        IEnumerable<KeyValuePair<Guid, TerrabreakMenu>> expiredMenus = menuManager.Menus.Where(pair =>
            pair.Value.LastInteractionTime is DateTimeOffset lastInteractionTime &&
            lastInteractionTime.AddMinutes(15) < DateTimeOffset.UtcNow).ToList();
        foreach (KeyValuePair<Guid, TerrabreakMenu> pair in expiredMenus)
            if (pair.Value.LastInteractionTime is not DateTimeOffset lastInteractionTime)
            {
                // GC interaction time-less menus immediately, and fire a warning
                Log.Warning(
                    "Menu {guid} of type {typeName} lacks a last interaction time, garbage-collecting indiscriminately.",
                    pair.Key, pair.Value.GetType().Name);
                GarbageCollectMenu(pair.Key, pair.Value);
            }
            else if (lastInteractionTime.AddMinutes(5) < DateTimeOffset.UtcNow)
            {
                Log.Debug("Garbage-collecting menu {guid} of type {typeName}", pair.Key, pair.Value.GetType().Name);
                GarbageCollectMenu(pair.Key, pair.Value);
            }

        return Task.CompletedTask;
    }

    private void GarbageCollectMenu(Guid guid, TerrabreakMenu menu)
    {
        if (!menuManager.Menus.Remove(guid, out _))
        {
            Log.Error("Failed to garbage-collect menu {guid} of type {typeName}.", guid, menu.GetType().Name);
        }
    }
}
