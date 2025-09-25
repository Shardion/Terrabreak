using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Quartz;
using Shardion.Terrabreak.Features.Reminders;
using Shardion.Terrabreak.Services.Database;

namespace Shardion.Terrabreak.Features.ManagementTools;

[InstanceOwnerPrecondition<ApplicationCommandContext>]
[SlashCommand("management", "Maintenance and management tools for instance owners.",
    Contexts = [InteractionContextType.Guild], IntegrationTypes = [ApplicationIntegrationType.GuildInstall],
    DefaultGuildPermissions = Permissions.Administrator)]
public class ManagementToolsModule(
    IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory,
    ISchedulerFactory schedulerFactory,
    RestClient discord)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("impersonate", "Impersonate the bot.")]
    public async Task Impersonate(
        [SlashCommandParameter(Description = "The message to make the bot send.")]
        string message,
        [SlashCommandParameter(Description = "The channel to send the message in.")]
        TextChannel channel
    )
    {
        await channel.SendMessageAsync(message);

        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            .WithContent("Message sent.")
            .WithFlags(MessageFlags.Ephemeral)
        ));
    }

    [SubSlashCommand("import", "Import a Project Terrabreak 3.1 data dump.")]
    public async Task Import()
    {
        Task deferTask = RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));
        await DataImporter.Import(dbContextFactory, schedulerFactory, discord);
        await deferTask;
        await ModifyResponseAsync(message => message
            .WithContent("Import complete.")
            .WithFlags(MessageFlags.Ephemeral)
        );
    }

    [SubSlashCommand("historic-reminder", "Creates a historic reminder that lacks creation info.")]
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
        int seconds = 0,
        [SlashCommandParameter(Description = "If the historic reminder should include starting channel info.")]
        bool hasChannel = true
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

        ulong startingChannel = Context.Channel.Id;
        ulong startingUser = Context.User.Id;

        IJobDetail job = JobBuilder.Create<SendReminderJob>()
            .WithIdentity(
                $"job-{Guid.NewGuid().ToString()}",
                $"remindersFor{Context.User.Id.ToString(CultureInfo.InvariantCulture)}")
            .UsingJobData("Note", note)
            .UsingJobData("StartingUnixTimeSeconds", startTime.ToUnixTimeSeconds())
            .UsingJobData("StartingMessage", null)
            .UsingJobData("StartingChannel", hasChannel ? startingChannel.ToString(CultureInfo.InvariantCulture) : null)
            .UsingJobData("StartingUser", startingUser.ToString(CultureInfo.InvariantCulture))
            .UsingJobData("StartingServer", null)
            // We take an error if this is false, but I think we can manage a few REST errors for historic reminders...
            .UsingJobData("CanFollowup", true)
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
                    new TextDisplayProperties("### Historic reminder set!"),
                    new TextDisplayProperties($">>> {note}"),
                    new TextDisplayProperties($"-# Will remind you on <t:{reminderTime.ToUnixTimeSeconds()}:F>")
                ])
                .WithFlags(MessageFlags.IsComponentsV2)
                .WithAllowedMentions(AllowedMentionsProperties.None));
    }
}
