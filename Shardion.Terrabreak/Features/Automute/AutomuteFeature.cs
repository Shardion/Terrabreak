using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Serilog;
using Shardion.Terrabreak.Services.Discord;
using Shardion.Terrabreak.Services.Options;

namespace Shardion.Terrabreak.Features.Automute
{
    public class AutomuteFeature : ITerrabreakFeature
    {
        private const int MUTE_MINUTES = 5;

        public ConcurrentDictionary<ulong, MessageAutomuteStatus> MessageAutomuteTracker { get; } = [];

        private readonly OptionsManager _optionsManager;

        public AutomuteFeature(DiscordManager discordManager, OptionsManager optionsManager)
        {
            _optionsManager = optionsManager;

            discordManager.Client.ReactionAdded += async (cacheableMessage, cacheableChannel, reaction) =>
            {
                if (await cacheableChannel.GetOrDownloadAsync() is not ITextChannel channel)
                {
                    Log.Debug("Did not handle reaction on message {0}, channel is not guild text channel", reaction.MessageId);
                    return;
                }
                AutomuteOptions automuteOptions = _optionsManager.Get<AutomuteOptions>(null, channel.GuildId);
                if (!automuteOptions.Enabled)
                {
                    Log.Debug("Did not handle reaction on message {0}, server disabled automute", reaction.MessageId);
                    return;
                }

                MessageAutomuteStatus automuteStatus = IncrementCounterForValidMessage(reaction);
                Log.Debug("Handling reaction on message {0}", reaction.MessageId);
                if (automuteStatus.MuteReactors.Count >= automuteOptions.ReactionsRequired)
                {
                    Log.Debug("Message {0} has >= {1} automute reactions, muting author", reaction.MessageId, automuteOptions.ReactionsRequired);
                    _ = Task.Run(async () => await MuteForValidMessage(cacheableMessage, channel));
                }

            };
        }

        private MessageAutomuteStatus IncrementCounterForValidMessage(SocketReaction reaction)
        {
            MessageAutomuteStatus automuteStatus = MessageAutomuteTracker.GetOrAdd(reaction.MessageId, new MessageAutomuteStatus()
            {
                Id = reaction.MessageId,
                MuteReactors = [],
                MuteTriggered = false,
            });

            // TODO: this would be better if there were concurrent sets... do those exist?
            if (automuteStatus.MuteReactors.Contains(reaction.UserId))
            {
                Log.Debug("Dropped automute reaction on message {0}, user {1} has already reacted to this message", automuteStatus.Id, reaction.UserId);
                return automuteStatus;
            }

            automuteStatus.MuteReactors.Add(reaction.UserId);
            MessageAutomuteTracker[reaction.MessageId] = automuteStatus;

            return automuteStatus;
        }

        private async Task MuteForValidMessage(Cacheable<IUserMessage, ulong> cacheableMessage, ITextChannel channel)
        {
            if (!MessageAutomuteTracker.TryGetValue(cacheableMessage.Id, out MessageAutomuteStatus? nullableAutomuteStatus) || nullableAutomuteStatus is not MessageAutomuteStatus automuteStatus)
            {
                Log.Error("BUG! Tried to automute user for a message I don't know about.");
                return;
            }

            if (automuteStatus.MuteTriggered)
            {
                Log.Debug("Message {0} author has already been automuted for this message, not muting again", cacheableMessage.Id);
                return;
            }

            automuteStatus.MuteTriggered = true;
            MessageAutomuteTracker[cacheableMessage.Id] = automuteStatus;

            // technically slightly unoptimal and could be started earlier,
            // but it's by a third of a millisecond or something silly like that
            IUserMessage message = await cacheableMessage.GetOrDownloadAsync();
            IGuildUser messageAuthor = await channel.GetUserAsync(message.Author.Id);

            Task modifyTask = messageAuthor.ModifyAsync((messageAuthorProperties) =>
            {
                DateTimeOffset baseOffset = messageAuthorProperties.TimedOutUntil.GetValueOrDefault() ?? DateTimeOffset.UtcNow;
                messageAuthorProperties.TimedOutUntil = baseOffset.AddMinutes(MUTE_MINUTES);
            });
        }

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }
    }
}
