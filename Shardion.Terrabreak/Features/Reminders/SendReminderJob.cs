using System.Globalization;
using System.Threading.Tasks;
using NetCord;
using NetCord.Rest;
using Quartz;
using Serilog;
using Shardion.Terrabreak.Services.Identity;

namespace Shardion.Terrabreak.Features.Reminders;

public class SendReminderJob(RestClient discord, IdentityOptions identity) : IJob
{
    // these are injected automatically
    public required string Note { private get; set; }
    public required string StartingUnixTimeSeconds { private get; set; }
    public required string? StartingMessage { private get; set; }
    public required string? StartingChannel { private get; set; }
    public required string StartingUser { private get; set; }
    public required string? StartingServer { private get; set; }
    public required bool CanFollowup { private get; set; }

    public async Task Execute(IJobExecutionContext context)
    {
        long startingUnixTimeSeconds = long.Parse(StartingUnixTimeSeconds, CultureInfo.InvariantCulture);
        ulong startingUser = ulong.Parse(StartingUser, CultureInfo.InvariantCulture);
        ulong? startingChannel = StartingChannel is not null
            ? ulong.Parse(StartingChannel, CultureInfo.InvariantCulture)
            : null;
        ulong? startingMessage = StartingMessage is not null
            ? ulong.Parse(StartingMessage, CultureInfo.InvariantCulture)
            : null;

        if (startingMessage is not ulong presentStartingMessage || startingChannel is not ulong presentStartingChannel)
        {
            Log.Warning(
                "Reminder has no starting message ID, treating as historic reminder from Project Terrabreak 3.1 or earlier. This is a bug for new reminders.");
            await RemindHistoric(startingUnixTimeSeconds, startingChannel, startingUser);
        }
        else
        {
            await Remind(startingUnixTimeSeconds, presentStartingChannel, startingUser, presentStartingMessage,
                StartingServer);
        }
    }

    public async Task Remind(long startingUnixTimeSeconds, ulong startingChannel, ulong startingUser,
        ulong startingMessage, string? startingServer)
    {
        if (CanFollowup)
        {
            MessageProperties reminderFollowupMessage = BuildReminderMessage(ReminderMessageTarget.OriginalChannel,
                ReminderMessageStyle.AsReply, startingUnixTimeSeconds,
                startingChannel.ToString(CultureInfo.InvariantCulture), startingUser, startingMessage,
                startingServer);
            try
            {
                await discord.SendMessageAsync(startingChannel, reminderFollowupMessage);
            }
            catch (RestException e)
            {
                Log.Error(
                    "Got a REST exception while sending a reminder message in the original channel, falling back to DMs. Original exception below...");
                Log.Error("{error}", e.ToString());

                MessageProperties reminderSeperateMessage = BuildReminderMessage(
                    ReminderMessageTarget.DirectMessageChannel,
                    ReminderMessageStyle.AsSeperateMessage, startingUnixTimeSeconds,
                    startingChannel.ToString(CultureInfo.InvariantCulture), startingUser, startingMessage,
                    startingServer);
                DMChannel userDms = await discord.GetDMChannelAsync(startingUser);
                await userDms.SendMessageAsync(reminderSeperateMessage);
            }
        }
        else
        {
            MessageProperties reminderSeperateMessage = BuildReminderMessage(ReminderMessageTarget.DirectMessageChannel,
                ReminderMessageStyle.AsSeperateMessage, startingUnixTimeSeconds,
                startingChannel.ToString(CultureInfo.InvariantCulture), startingUser, startingMessage,
                startingServer);
            DMChannel userDms = await discord.GetDMChannelAsync(startingUser);
            await userDms.SendMessageAsync(reminderSeperateMessage);
        }
    }

    public async Task RemindHistoric(long startingUnixTimeSeconds, ulong? startingChannel, ulong startingUser)
    {
        if (CanFollowup && startingChannel is ulong presentStartingChannel)
        {
            MessageProperties reminderFollowupMessage = BuildHistoricReminderMessage(
                ReminderMessageTarget.OriginalChannel, startingUnixTimeSeconds,
                presentStartingChannel.ToString(CultureInfo.InvariantCulture), startingUser);
            try
            {
                await discord.SendMessageAsync(presentStartingChannel, reminderFollowupMessage);
            }
            catch (RestException e)
            {
                Log.Error(
                    "Got a REST exception while sending a historic reminder message in the original channel, falling back to DMs. Original exception below...");
                Log.Error("{error}", e.ToString());

                MessageProperties reminderSeperateMessage = BuildHistoricReminderMessage(
                    ReminderMessageTarget.DirectMessageChannel, startingUnixTimeSeconds,
                    presentStartingChannel.ToString(CultureInfo.InvariantCulture), startingUser);
                DMChannel userDms = await discord.GetDMChannelAsync(startingUser);
                await userDms.SendMessageAsync(reminderSeperateMessage);
            }
        }
        else
        {
            MessageProperties reminderSeperateMessage =
                BuildHistoricReminderMessage(ReminderMessageTarget.DirectMessageChannel, startingUnixTimeSeconds, null,
                    startingUser);
            DMChannel userDms = await discord.GetDMChannelAsync(startingUser);
            await userDms.SendMessageAsync(reminderSeperateMessage);
        }
    }

    private MessageProperties BuildReminderMessage(ReminderMessageTarget target, ReminderMessageStyle style,
        long startingUnixTimeSeconds, string startingChannel, ulong startingUser,
        ulong startingMessage, string? startingServer)
    {
        bool shouldMentionInHeader = target == ReminderMessageTarget.OriginalChannel && startingServer is not null;
        TextDisplayProperties header =
            new(shouldMentionInHeader ? $"### <@{startingUser}> Reminder!" : "### Reminder!");

        TextDisplayProperties? footer = null;
        if (style == ReminderMessageStyle.AsSeperateMessage)
        {
            string footerServerId = startingServer ?? "@me";
            string footerMessageId = startingMessage.ToString(CultureInfo.InvariantCulture);
            footer = new TextDisplayProperties(
                $"-# Set by [this command](<https://discord.com/channels/{footerServerId}/{startingChannel}/{footerMessageId}>) on <t:{startingUnixTimeSeconds}:F>");
        }

        MessageProperties message = new MessageProperties()
            .WithFlags(MessageFlags.IsComponentsV2)
            .WithAllowedMentions(new AllowedMentionsProperties
            {
                AllowedUsers = null,
                AllowedRoles = null,
                Everyone = false,
                ReplyMention = true
            })
            .WithMessageReference(MessageReferenceProperties.Reply(startingMessage, false))
            .WithComponents([
                header,
                new TextDisplayProperties($">>> {Note}")
            ]);

        if (footer is not null) message.AddComponents(footer);

        return message;
    }

    private MessageProperties BuildHistoricReminderMessage(ReminderMessageTarget target, long startingUnixTimeSeconds,
        string? startingChannel, ulong startingUser)
    {
        bool shouldMentionInHeader = target == ReminderMessageTarget.OriginalChannel;
        bool shouldLinkChannelInFooter =
            target == ReminderMessageTarget.DirectMessageChannel && startingChannel is not null;
        TextDisplayProperties header =
            new(shouldMentionInHeader ? $"### <@{startingUser}> Reminder!" : "### Reminder!");
        TextDisplayProperties footer = new(shouldLinkChannelInFooter
            ? $"-# Set in <#{startingChannel}> without creation info on <t:{startingUnixTimeSeconds}:F>"
            : $"-# Set without creation info by {identity.BotName} 3.1 on <t:{startingUnixTimeSeconds}:F>");

        MessageProperties message = new MessageProperties()
            .WithFlags(MessageFlags.IsComponentsV2)
            .WithAllowedMentions(new AllowedMentionsProperties
            {
                AllowedUsers = null,
                AllowedRoles = null,
                Everyone = false,
                ReplyMention = true
            })
            .WithComponents([
                header,
                new TextDisplayProperties($">>> {Note}"),
                footer
            ]);

        return message;
    }
}
