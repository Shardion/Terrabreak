using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NetCord.Rest;
using Quartz;
using Serilog;
using Shardion.Terrabreak.Features.Bags;
using Shardion.Terrabreak.Features.Reminders;
using Shardion.Terrabreak.Services.Database;

namespace Shardion.Terrabreak.Features.ManagementTools;

public class DataImporter
{
    public static async Task Import(IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory,
        ISchedulerFactory schedulerFactory, RestClient discord)
    {
        await ImportReminders(dbContextFactory, schedulerFactory, discord);
        await ImportBags(dbContextFactory);
    }

    public static async Task ImportReminders(IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory,
        ISchedulerFactory schedulerFactory, RestClient discord)
    {
        await using FileStream file =
            File.OpenRead(Path.Join(Entrypoint.GetConfigurationDirectoryPath(), "dump-timeouts.json"));
        IEnumerable<HistoricTimeout>? timeouts =
            await JsonSerializer.DeserializeAsync<IEnumerable<HistoricTimeout>>(file);
        if (timeouts is null) throw new JsonException("Timeouts dump is null after deserialization!!");

        foreach (HistoricTimeout timeout in timeouts)
        {
            if (timeout.Identifier != "reminder" || timeout.GetDeserializedData() is not HistoricReminderInfo reminder)
            {
                Log.Information("IMPORTER: Skipped non-reminder timeout with identifier {identifier}",
                    timeout.Identifier);
                continue;
            }

            if (timeout.ExpiryProcessed)
                // Reminder was already processed by 3.1, but wasn't garbage collected before the database dump
                continue;

            Task<IScheduler> schedulerTask = schedulerFactory.GetScheduler();

            IJobDetail job = JobBuilder.Create<SendReminderJob>()
                .WithIdentity(
                    $"job-{timeout.Id}",
                    $"remindersFor{reminder.UserId}")
                .UsingJobData("Note", reminder.Note)
                .UsingJobData("StartingUnixTimeSeconds", reminder.StartTime.ToUnixTimeSeconds())
                .UsingJobData("StartingMessage", null)
                .UsingJobData("StartingChannel", reminder.ChannelId?.ToString(CultureInfo.InvariantCulture))
                .UsingJobData("StartingUser", reminder.UserId.ToString(CultureInfo.InvariantCulture))
                .UsingJobData("StartingServer", null)
                .UsingJobData("CanFollowup", true)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(
                    $"trigger-{Guid.NewGuid().ToString()}",
                    $"remindersFor{reminder.UserId}")
                .StartAt(timeout.ExpirationDate)
                .Build();

            IScheduler scheduler = await schedulerTask;
            try
            {
                await scheduler.ScheduleJob(job, trigger);
            }
            catch (JobPersistenceException)
            {
                Log.Warning($"Tried to import a duplicate of reminder {timeout.Id}, skipping.");
            }
        }
    }

    public static async Task ImportBags(IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory)
    {
        await using FileStream file =
            File.OpenRead(Path.Join(Entrypoint.GetConfigurationDirectoryPath(), "dump-bags.json"));
        IEnumerable<HistoricBag>? bags = await JsonSerializer.DeserializeAsync<IEnumerable<HistoricBag>>(file);
        if (bags is null) throw new JsonException("Bags dump is null after deserialization!!");

        TerrabreakDatabaseContext db = await dbContextFactory.CreateDbContextAsync();
        await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync();
        List<Bag> updatedBags = [];
        List<Bag> addedBags = [];
        // TODO: This code really isn't the nicest, lots of nested loops, but I don't think there's a single self-hoster
        //       who would benefit from this code being faster, so...?
        foreach (HistoricBag historicBag in bags)
            // Merge bags with the same name and owner, but different IDs
            if (db.GetBag(historicBag.OwnerId, historicBag.Name) is Bag existingBag)
            {
                // Ignore bags that also share the ID
                if (existingBag.Id == historicBag.Id) continue;
                foreach (string entry in historicBag.Entries)
                    existingBag.Entries.Add(new BagEntry
                        {
                            Text = entry,
                            Bag = existingBag
                        }
                    );
                updatedBags.Add(existingBag);
            }
            else
            {
                Bag newBag = new()
                {
                    // Historic bags keep their IDs
                    Id = historicBag.Id,
                    Name = historicBag.Name,
                    OwnerId = historicBag.OwnerId,
                    Entries = []
                };
                foreach (string entry in historicBag.Entries)
                    newBag.Entries.Add(new BagEntry
                    {
                        Bag = newBag,
                        Text = entry
                    });
                addedBags.Add(newBag);
            }

        await db.AddRangeAsync(addedBags);
        db.UpdateRange(updatedBags);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
