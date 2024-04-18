using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using LiteDB;
using Serilog;
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
        public async Task Remind(string note, int days = 0, int hours = 0, int minutes = 0, int seconds = 0)
        {
            TimeSpan offset = new(days, hours, minutes, seconds);
            if (offset <= TimeSpan.Zero)
            {
                await Context.Interaction.RespondAsync("Invalid time.");
            }

            DateTimeOffset offsettedTime = DateTimeOffset.UtcNow.Add(offset);

            BsonDocument timerInfo = new()
            {
                ["note"] = note,
                ["startTime"] = DateTimeOffset.UtcNow.UtcDateTime,
                ["uid"] = Context.User.Id.ToString(CultureInfo.InvariantCulture),
            };

            if (Context.Channel is not null)
            {
                timerInfo["cid"] = Context.Channel.Id.ToString(CultureInfo.InvariantCulture);
            }

            Timeout timeout = new()
            {
                Identifier = "reminder",
                ExpirationDate = offsettedTime,
                Data = timerInfo,
                ExpiryProcessed = false,
            };
            _timeoutManager.BeginTimeout(timeout);

            await Context.Interaction.RespondAsync($"Reminder set for **<t:{offsettedTime.ToUnixTimeSeconds()}:F>**!\n> {note}");
        }
    }
}
