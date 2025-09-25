using System.Threading.Tasks;
using Quartz;

namespace Shardion.Terrabreak.Services.Identity;

public class IdentityManager(ISchedulerFactory schedulerFactory, IdentityOptions options) : ITerrabreakService
{
    public IdentityOptions Options { get; } = options;

    public string? LastSplash { get; set; } = null;

    public async Task StartAsync()
    {
        // TODO: Ideally would never touch the DB, but could be made better by only adding the jobs if it doesn't exist
        IScheduler scheduler = await schedulerFactory.GetScheduler();
        await scheduler.DeleteJob(new JobKey("changeStatusJob", "statusRotation"));
        IJobDetail job = JobBuilder.Create<ChangeStatusJob>()
            .WithIdentity("changeStatusJob", "statusRotation")
            .Build();
        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("changeStatusTrigger", "statusRotation")
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInMinutes(30)
                .RepeatForever())
            .Build();

        await scheduler.ScheduleJob(job, trigger);
    }
}
