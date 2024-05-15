using Shardion.Terrabreak.Services.Timeout;
using Shardion.Terrabreak.Services.Discord;
using Discord;
using Serilog;
using System.Threading.Tasks;
using System;
using Discord.Net;
using System.Text.Json;

namespace Shardion.Terrabreak.Features.Reminders
{
    public class RemindersFeature : ITerrabreakFeature
    {
        private readonly DiscordManager _discordManager;
        private readonly TimeoutManager _timeoutManager;

        public RemindersFeature(DiscordManager discordManager, TimeoutManager timeoutManager)
        {
            _discordManager = discordManager;
            _timeoutManager = timeoutManager;
            _timeoutManager.TimeoutExpired += async (timeout) =>
            {
                try
                {
                    if (timeout.Identifier != "reminder")
                    {
                        return;
                    }

                    using JsonDocument document = JsonDocument.Parse(timeout.Data);

                    if (document.RootElement.ValueKind != JsonValueKind.Object)
                    {
                        throw new JsonException("JSON is so badly malformed that I have no idea what to do with it");
                    }

                    ulong? uid;
                    if (!document.RootElement.TryGetProperty("UserId", out JsonElement unparsedUid) || unparsedUid.ValueKind != JsonValueKind.String || !ulong.TryParse(unparsedUid.ToString(), out ulong parsedUid))
                    {
                        uid = null;
                    }
                    else
                    {
                        uid = parsedUid;
                    }

                    ValueTask<IUser?> targetUserTask;
                    if (uid is not ulong validUid)
                    {
                        targetUserTask = ValueTask.FromResult<IUser?>(null);
                    }
                    else
                    {
                        targetUserTask = _discordManager.Client.GetUserAsync(validUid);
                    }

                    ulong? cid;
                    if (!document.RootElement.TryGetProperty("ChannelId"u8, out JsonElement unparsedCid) || unparsedCid.ValueKind != JsonValueKind.String || !ulong.TryParse(unparsedCid.GetString(), out ulong parsedCid))
                    {
                        cid = null;
                    }
                    else
                    {
                        cid = parsedCid;
                    }

                    ValueTask<IChannel?> targetChannelTask;
                    if (cid is not ulong validCid)
                    {
                        targetChannelTask = ValueTask.FromResult<IChannel?>(null);
                    }
                    else
                    {
                        targetChannelTask = _discordManager.Client.GetChannelAsync(validCid);
                    }

                    DateTimeOffset? startTime;
                    if (!document.RootElement.TryGetProperty("StartTime"u8, out JsonElement unparsedStartTime))
                    {
                        startTime = null;
                    }
                    else
                    {
                        try
                        {
                            startTime = unparsedStartTime.Deserialize<DateTimeOffset>();
                        }
                        catch (JsonException)
                        {
                            startTime = null;
                        }
                    }

                    string? reminderNote;
                    if (!document.RootElement.TryGetProperty("Note"u8, out JsonElement note) || note.ValueKind != JsonValueKind.String)
                    {
                        reminderNote = null;
                    }
                    else
                    {
                        reminderNote = note.GetString();
                    }

                    string reminderTimeLine;
                    if (startTime is not DateTimeOffset validStartTime)
                    {
                        reminderTimeLine = "**Reminder from unknown time!**";
                    }
                    else
                    {
                        reminderTimeLine = $"**Reminder from <t:{validStartTime.ToUnixTimeSeconds()}:F>**!";
                    }

                    string reminderTextLine;
                    if (reminderNote is null)
                    {
                        reminderTextLine = "I failed to load the note for this reminder. Oops. You might be able to recover it by finding the original message.";
                    }
                    else
                    {
                        reminderTextLine = $"> {reminderNote}";
                    }

                    // start preparing message content
                    AllowedMentions messageMentions = new();

                    string reminderMentionLine;
                    IUser? targetUser = await targetUserTask;
                    if (targetUser is null)
                    {
                        reminderMentionLine = "*Unknown user. If you know who this reminder was for, ping them!*";
                    }
                    else
                    {
                        reminderMentionLine = targetUser.Mention;
                        messageMentions.UserIds.Add(targetUser.Id);
                    }

                    string messageContent = $"{reminderMentionLine}\n{reminderTimeLine}\n{reminderTextLine}";

                    IChannel? targetChannel = await targetChannelTask;
                    if (targetChannel is IMessageChannel validChannel)
                    {
                        try
                        {
                            await validChannel.SendMessageAsync(messageContent, allowedMentions: messageMentions);
                        }
                        catch (HttpException channelDeliveryException)
                        {
                            Log.Error($"Got error {nameof(channelDeliveryException)} while trying to send reminder {timeout.Id} to channel {targetChannel.Id}. Trying again in DMs.");
                            Log.Error(channelDeliveryException.ToString());
                            if (targetUser is not null)
                            {
                                try
                                {
                                    await targetUser.SendMessageAsync(messageContent, allowedMentions: messageMentions);
                                }
                                catch (HttpException dmsDeliveryException)
                                {
                                    Log.Error($"Got error {nameof(dmsDeliveryException)} while trying to send reminder {timeout.Id} to DMs of user {targetUser.Id}. Dropping!");
                                }
                            }
                            else
                            {
                                Log.Error($"Dropped reminder {timeout.Id} as no valid non-erroring delivery area could be found!");
                            }
                        }
                    }
                    else
                    {
                        if (targetUser is not null)
                        {
                            try
                            {
                                await targetUser.SendMessageAsync(messageContent, allowedMentions: messageMentions);
                            }
                            catch (HttpException dmsDeliveryException)
                            {
                                Log.Error($"Got error {nameof(dmsDeliveryException)} while trying to send reminder {timeout.Id} to DMs of user {targetUser.Id}. Dropping!");
                            }
                        }
                        else
                        {
                            Log.Error($"Dropped reminder {timeout.Id} as no valid non-erroring delivery area could be found!");
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            };
        }

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }
    }
}
