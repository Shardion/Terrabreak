using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Quartz;

namespace Shardion.Terrabreak.Services.Menuing;

public class MenuManager(ISchedulerFactory schedulerFactory) : ITerrabreakService
{
    public ConcurrentDictionary<Guid, TerrabreakMenu> Menus { get; } = [];

    public Guid ActivateMenu(TerrabreakMenu menuMessage)
    {
        Guid menuGuid = Guid.NewGuid();
        Menus[menuGuid] = menuMessage;
        return menuGuid;
    }

    public async Task StartAsync()
    {
        // TODO: Ideally would never touch the DB, but could be made better by only adding the jobs if it doesn't exist
        IScheduler scheduler = await schedulerFactory.GetScheduler();
        await scheduler.DeleteJob(new JobKey("collectGarbageMenusJob", "menuManager"));
        IJobDetail job = JobBuilder.Create<CollectGarbageMenusJob>()
            .WithIdentity("collectGarbageMenusJob", "menuManager")
            .Build();
        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("collectGarbageMenusTrigger", "menuManager")
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInMinutes(15)
                .RepeatForever())
            .Build();

        await scheduler.ScheduleJob(job, trigger);
    }
}
