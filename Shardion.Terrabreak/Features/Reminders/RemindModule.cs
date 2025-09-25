using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Quartz;
using Serilog;

namespace Shardion.Terrabreak.Features.Reminders;

public class RemindModule(ISchedulerFactory schedulerFactory) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("remind", "Pings you at a specified time in the future with a specified note.")]
    public async Task CreateReminder(
        [SlashCommandParameter(Description = "The note that you will be reminded with.")]
        string note,
        [SlashCommandParameter(Description = "A number of days to add to the expiry time.")]
        int days = 0,
        [SlashCommandParameter(Description = "A number of hours to add to the expiry time.")]
        int hours = 0,
        [SlashCommandParameter(Description = "A number of minutes to add to the expiry time.")]
        int minutes = 0,
        [SlashCommandParameter(Description = "A number of seconds to add to the expiry time.")]
        int seconds = 0
    )
    {
        TimeSpan offset = new(days, hours, minutes, seconds);
        if (offset <= TimeSpan.Zero)
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("Invalid time.")
                .WithFlags(MessageFlags.Ephemeral)));
            return;
        }

        DateTimeOffset startTime = DateTimeOffset.UtcNow;
        DateTimeOffset reminderTime = DateTimeOffset.UtcNow.Add(offset);

        Task deferral = RespondAsync(InteractionCallback.DeferredMessage());
        IScheduler scheduler = await schedulerFactory.GetScheduler();

        await deferral;
        RestMessage deferralMessage = await GetResponseAsync();

        ulong startingMessage = deferralMessage.Id;
        ulong startingChannel = Context.Channel.Id;
        ulong startingUser = Context.User.Id;
        ulong? startingServer = Context.Guild?.Id;
        bool canFollowup = false;

        // if we are installed in a context where we can followup to a message
        if (Context.Interaction.AuthorizingIntegrationOwners.TryGetValue(ApplicationIntegrationType.GuildInstall,
                out ulong contextServer))
        {
            canFollowup = true;
            // 0 is magic: if a key of GuildInstall is set to 0, it means that
            // the interaction happened in the user's DMs with the bot
            if (contextServer == 0) startingServer = null;
        }

        IJobDetail job = JobBuilder.Create<SendReminderJob>()
            .WithIdentity(
                $"job-{Guid.NewGuid().ToString()}",
                $"remindersFor{Context.User.Id.ToString(CultureInfo.InvariantCulture)}")
            .UsingJobData("Note", note)
            .UsingJobData("StartingUnixTimeSeconds", startTime.ToUnixTimeSeconds())
            .UsingJobData("StartingMessage", startingMessage.ToString(CultureInfo.InvariantCulture))
            .UsingJobData("StartingChannel", startingChannel.ToString(CultureInfo.InvariantCulture))
            .UsingJobData("StartingUser", startingUser.ToString(CultureInfo.InvariantCulture))
            .UsingJobData("StartingServer", startingServer?.ToString(CultureInfo.InvariantCulture))
            .UsingJobData("CanFollowup", canFollowup)
            .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity(
                $"trigger-{Guid.NewGuid().ToString()}",
                $"remindersFor{Context.User.Id.ToString(CultureInfo.InvariantCulture)}")
            .StartAt(reminderTime)
            .Build();

        await scheduler.ScheduleJob(job, trigger);

        await ModifyResponseAsync(message =>
            message.WithComponents([
                    new TextDisplayProperties("### Reminder set!"),
                    new TextDisplayProperties($">>> {note}"),
                    new TextDisplayProperties($"-# Will remind you on <t:{reminderTime.ToUnixTimeSeconds()}:F>")
                ])
                .WithFlags(MessageFlags.IsComponentsV2)
                .WithAllowedMentions(AllowedMentionsProperties.None));
    }
}
