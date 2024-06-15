using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Shardion.Terrabreak.Services.Discord;
using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Services.Options;
using Shardion.Terrabreak.Services.Timeout;

namespace Shardion.Terrabreak.Features.Sudo
{
    [CommandContextType(InteractionContextType.Guild)]
    [IntegrationType(ApplicationIntegrationType.GuildInstall)]
    public class SudoModule : TerrabreakInteractionModuleBase
    {
        private readonly OptionsManager _options;
        private readonly DiscordManager _discord;
        private readonly TimeoutManager _timeout;
        private readonly IdentityManager _identity;

        public SudoModule(OptionsManager options, DiscordManager discord, TimeoutManager timeout, IdentityManager identity)
        {
            _options = options;
            _discord = discord;
            _timeout = timeout;
            _identity = identity;
        }

        [SlashCommand("sudo", "Temporarily gives you more permissions.")]
        public async Task Sudo()
        {
            SudoOptions opt = _options.Get<SudoOptions>(userId: null, serverId: Context.Guild.Id);

            if (!opt.SudoerUserIDs.Contains(Context.User.Id))
            {
                await RespondAsync("`You are not in the sudoers file. This incident will be reported.`", ephemeral: true);
                return;
            }

            DateTimeOffset enableTime = DateTimeOffset.UtcNow.AddSeconds(60);
            MessageBuilder message = new MessageBuilder()
                .WithEmbed(_identity.GetEmbedTemplate()
                    .WithTitle("Really elevate permissions?")
                    .WithDescription($"Are you sure you want to gain heightened permissions?\nButtons enable <t:{enableTime.ToUnixTimeSeconds()}:R>.")
                    .WithColor(0xff0000))
                .WithComponents(new ComponentBuilder()
                    .WithButton("Elevate", customId: ":oncoming_demon_god:", style: ButtonStyle.Danger, disabled: true)
                    .WithButton("Cancel", customId: ":thegamer:", style: ButtonStyle.Success, disabled: true))
                .WithAllowedMentions(null, null);

            await RespondAsync(message.Build());
            await Task.Delay(enableTime - DateTimeOffset.UtcNow);

            IUserMessage modifiedMessage = await ValidateButtons();
            if (await InteractionUtility.WaitForMessageComponentAsync(_discord.Client, modifiedMessage, TimeSpan.FromSeconds(30)) is null)
            {
                await InvalidateForInactivity();
            }
        }

        [ComponentInteraction("sudoCancel:*", true)]
        public async Task SudoCancel(ulong intendedUserId)
        {
            if (Context.User.Id != intendedUserId)
            {
                await RespondAsync("You are not worthy.", ephemeral: true);
                return;
            }

            await InvalidateForCancellation();
        }

        [ComponentInteraction("sudoConfirm:*", true)]
        public async Task SudoConfirm(ulong intendedUserId)
        {
            if (Context.User.Id != intendedUserId)
            {
                await RespondAsync("You are not worthy.", ephemeral: true);
                return;
            }

            SudoOptions opt = _options.Get<SudoOptions>(userId: null, serverId: Context.Guild.Id);

            if (opt.SudoRoleId is not ulong sudoRoleId)
            {
                return;
            }

            Task[] elevateTasks =
            [
                _discord.RestClient.AddRoleAsync(Context.Guild.Id, Context.User.Id, sudoRoleId),
                InvalidateForConfirmation(),
            ];

            SudoExpiryInfo expiryInfo = new()
            {
                GuildId = Context.Guild.Id.ToString(CultureInfo.InvariantCulture),
                UserId = Context.User.Id.ToString(CultureInfo.InvariantCulture),
            };

            Timeout timeout = new()
            {
                Identifier = "sudo",
                ExpirationDate = DateTimeOffset.UtcNow.AddMinutes(10),
                Data = JsonSerializer.SerializeToUtf8Bytes(expiryInfo),
                ExpiryProcessed = false,
            };

            _timeout.BeginTimeout(timeout);

            await Task.WhenAll(elevateTasks);
        }

        public Task InvalidateForCancellation()
        {
            if (Context.Interaction is not IComponentInteraction componentInteraction)
            {
                return Task.CompletedTask;
            }

            return componentInteraction.UpdateAsync((m) =>
            {
                m.Embed = _identity.GetEmbedTemplate()
                    .WithTitle("Really elevate permissions?")
                    .WithDescription($"Are you sure you want to gain heightened permissions?")
                    .WithFooter(new EmbedFooterBuilder()
                    {
                        IconUrl = "https://anomaly.tail354c3.ts.net/assets/bulb.png",
                        Text = "You cancelled the operation.",
                    })
                    .WithColor(0xff0000)
                    .Build();
                m.Components = new ComponentBuilder()
                    .WithButton("Elevate", customId: ":oncoming_demon_god:", style: ButtonStyle.Danger, disabled: true)
                    .WithButton("Cancel", customId: ":thegamer:", style: ButtonStyle.Success, disabled: true)
                    .Build();
            });
        }

        public Task InvalidateForConfirmation()
        {
            SudoOptions opt = _options.Get<SudoOptions>(userId: null, serverId: Context.Guild.Id);

            if (Context.Interaction is not IComponentInteraction componentInteraction)
            {
                return Task.CompletedTask;
            }

            return componentInteraction.UpdateAsync((m) =>
            {
                m.Embed = _identity.GetEmbedTemplate()
                    .WithTitle("Permissions elevated")
                    .WithDescription($"You have been granted the role <@&{opt.SudoRoleId}>.\nIt will expire <t:{DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds()}:R>.")
                    .WithColor(0xff0000)
                    .Build();
                m.Components = new ComponentBuilder()
                    .WithButton("Elevate", customId: ":oncoming_demon_god:", style: ButtonStyle.Danger, disabled: true)
                    .WithButton("Cancel", customId: ":thegamer:", style: ButtonStyle.Success, disabled: true)
                    .Build();
            });
        }

        public Task<IUserMessage> InvalidateForInactivity()
        {
            return Context.Interaction.ModifyOriginalResponseAsync((m) =>
            {
                m.Embed = _identity.GetEmbedTemplate()
                    .WithTitle("Really elevate permissions?")
                    .WithDescription($"Are you sure you want to gain heightened permissions?")
                    .WithFooter(new EmbedFooterBuilder()
                    {
                        IconUrl = "https://anomaly.tail354c3.ts.net/assets/bulb.png",
                        Text = "Cancelled due to inactivity.",
                    })
                    .WithColor(0xff0000)
                    .Build();
                m.Components = new ComponentBuilder()
                    .WithButton("Elevate", customId: ":oncoming_demon_god:", style: ButtonStyle.Danger, disabled: true)
                    .WithButton("Cancel", customId: ":thegamer:", style: ButtonStyle.Success, disabled: true)
                    .Build();
            });
        }

        public Task<IUserMessage> ValidateButtons()
        {
            return Context.Interaction.ModifyOriginalResponseAsync((m) =>
            {
                m.Components = new ComponentBuilder()
                    .WithButton("Elevate", customId: $"sudoConfirm:{Context.User.Id}", style: ButtonStyle.Danger)
                    .WithButton("Cancel", customId: $"sudoCancel:{Context.User.Id}", style: ButtonStyle.Success)
                    .Build();
            });
        }
    }
}
