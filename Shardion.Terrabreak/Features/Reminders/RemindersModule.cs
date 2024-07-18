using System;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Shardion.Terrabreak.Services.Timeout;

namespace Shardion.Terrabreak.Features.Reminders
{
    [CommandContextType(InteractionContextType.BotDm, InteractionContextType.PrivateChannel, InteractionContextType.Guild)]
    [IntegrationType(ApplicationIntegrationType.UserInstall, ApplicationIntegrationType.GuildInstall)]
    public class RemindersModule : InteractionModuleBase
    {
        private readonly TimeoutManager _timeoutManager;

        public RemindersModule(TimeoutManager timeoutManager)
        {
            _timeoutManager = timeoutManager;
        }

        [SlashCommand("remind", "Pings you at a specified time in the future with a specified note.")]
        public async Task CreateReminder(
            [Summary(description: "The note that you will be pinged with.")] string note,
            [Summary(description: "A number of days to add to the expiry time.")] int days = 0,
            [Summary(description: "A number of hours to add to the expiry time.")] int hours = 0,
            [Summary(description: "A number of minutes to add to the expiry time.")] int minutes = 0,
            [Summary(description: "A number of seconds to add to the expiry time.")] int seconds = 0
        )
        {
            TimeSpan offset = new(days, hours, minutes, seconds);
            if (offset <= TimeSpan.Zero)
            {
                await RespondAsync("Invalid time.", ephemeral: true);
                return;
            }

            DateTimeOffset offsettedTime = DateTimeOffset.UtcNow.Add(offset);

            ReminderInfo timerInfo = new()
            {
                Note = note,
                StartTime = DateTimeOffset.UtcNow,
                UserId = Context.User.Id.ToString(CultureInfo.InvariantCulture),
            };

            if (Context.Channel is ITextChannel)
            {
                timerInfo.ChannelId = Context.Channel.Id.ToString(CultureInfo.InvariantCulture);
            }

            Timeout timeout = new()
            {
                Identifier = "reminder",
                ExpirationDate = offsettedTime,
                Data = JsonSerializer.SerializeToUtf8Bytes(timerInfo),
                ExpiryProcessed = false,
                ShouldRetry = true,
            };
            _timeoutManager.BeginTimeout(timeout);

            await RespondAsync($"Reminder set for **<t:{offsettedTime.ToUnixTimeSeconds()}:F>**!\n> {note}");
        }
    }
}
