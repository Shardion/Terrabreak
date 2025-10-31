using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;
using Serilog;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Equipment;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Player;
using Shardion.Terrabreak.Features.ShusoDivineReunion.Takeover;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Battle;

public class LiberationMenu(IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory, IReadOnlyCollection<DiscordPlayer> discordPlayers, BattleEngine battle, TakeoverManager takeoverManager, LiberationFeature liberationFeature) : TerrabreakMenu
{
    public static FrozenDictionary<Debuff, string> DebuffEmojis { get; } = new Dictionary<Debuff, string>
    {
        [Debuff.Sealed] = "<:sealed:1426021766484201472>",
        [Debuff.Burning] = "<:burning:1426021751007215668>",
        [Debuff.Weakened] = "<:weakened:1426021779335282728>",
        [Debuff.Subjugation] = "<:subjugation:1426021776386949172>",
    }.ToFrozenDictionary();

    public BattleResult? Result { get; set; }

    public override async Task OnCreate(ApplicationCommandContext context, Guid guid)
    {
        battle.Start();
        foreach (DiscordPlayer player in discordPlayers)
        {
            liberationFeature.PlayersInBattles.AddOrUpdate(player.UserId, _ => true, (_, _) => true);
        }

        await base.OnCreate(context, guid);
        _ = Task.Run(async () => await LoopToCompletion(context));
    }

    public override async Task OnReplace(IComponentInteractionContext context, Guid guid)
    {
        battle.Start();
        foreach (DiscordPlayer player in discordPlayers)
        {
            liberationFeature.PlayersInBattles.AddOrUpdate(player.UserId, _ => true, (_, _) => true);
        }

        await base.OnReplace(context, guid);
        _ = Task.Run(async () => await LoopToCompletion(context));
    }

    public override Task<MenuMessage> BuildMessage()
    {
        List<IComponentContainerComponentProperties> battleUi = [];
        int enemyHealth = Convert.ToInt32(Math.Round((decimal)battle.EnemyState.Health / battle.Enemy.HealthMax * 100));
        battleUi.Add(new TextDisplayProperties($"<:nothing:1424096863585308672> {ProduceHealthBar(enemyHealth)} `{enemyHealth}%` {battle.Enemy.Name}"));

        StringBuilder logLines = new();
        foreach (string line in battle.BattleLog)
        {
            logLines.AppendLine(line);
        }
        battleUi.Add(new TextDisplayProperties($">>> {logLines}"));

        foreach ((IPlayer player, PlayerState playerState) in battle.Players)
        {
            int playerHealth = Convert.ToInt32(Math.Round((decimal)playerState.Health / player.HealthMax * 100));
            Debuff? maybeDebuff = null;
            if (playerState.Debuffs.Count > 0)
            {
                maybeDebuff = playerState.Debuffs.First();
            }
            string debuffEmoji = maybeDebuff is Debuff debuff ? DebuffEmojis[debuff] : "<:nothing:1424096863585308672>";
            battleUi.Add(new TextDisplayProperties($"{debuffEmoji} {ProduceHealthBar(playerHealth)} `{playerHealth}%` {player.Name}"));
        }

        battleUi.Add(new ComponentSeparatorProperties());
        battleUi.Add(new ActionRowProperties([
            new ButtonProperties($"menu:{MenuGuid}:heal", EmojiProperties.Custom(1429636573598584842), ButtonStyle.Secondary),
            new ButtonProperties($"menu:{MenuGuid}:cure", EmojiProperties.Custom(1429636571862274129), ButtonStyle.Secondary),
            new ButtonProperties($"menu:{MenuGuid}:ribbon", EmojiProperties.Custom(1429603809755398154), ButtonStyle.Secondary),
            new ButtonProperties($"menu:{MenuGuid}:no-item", EmojiProperties.Custom(1430002810635943936), ButtonStyle.Secondary),
        ]));

        return Task.FromResult(new MenuMessage([
            new ComponentContainerProperties(battleUi)
        ]));
    }

    public async Task LoopToCompletion(IInteractionContext context)
    {
        try
        {
            // TODO: `while` is scary...
            while (Result is null)
            {
                // Manually stave off menu GC
                LastInteractionTime = DateTimeOffset.UtcNow;

                await Task.Delay(TimeSpan.FromSeconds(5));
                Result = battle.Turn();

                MenuMessage message = await BuildMessage();
                Task editTask = ModifyResponseAsync(context, responseMessage => responseMessage
                    .WithAttachments(message.Attachments)
                    .WithComponents(message.Components)
                    .WithFlags(message.Flags | MessageFlags.IsComponentsV2)
                    .WithAllowedMentions(message.AllowedMentions));

                if (Result is not null)
                {
                    foreach (DiscordPlayer playerInBattle in discordPlayers)
                    {
                        _ = liberationFeature.PlayersInBattles.Remove(playerInBattle.UserId, out bool _);
                    }
                }

                if (Result is { Victor: Victor.Players } result)
                {
                    TerrabreakDatabaseContext dbContext = await dbContextFactory.CreateDbContextAsync();
                    Task relinquishChannelTask =
                        takeoverManager.RelinquishChannelAsync(dbContext, context.Interaction.Channel.Id);
                    List<DiscordPlayer> updatedPlayers = [];
                    foreach (DiscordPlayer oldPlayer in discordPlayers)
                    {
                        DiscordPlayer? newPlayer = dbContext.GetPlayer(oldPlayer.UserId);
                        if (newPlayer is null)
                        {
                            newPlayer = oldPlayer;
                        }

                        newPlayer.Credits += result.Payout;
                        if (newPlayer.StrongestEnemy is EnemyRecord playerStrongestEnemy)
                        {
                            if (playerStrongestEnemy.Enemy.TargetTotalPowerLevel < battle.Enemy.TargetTotalPowerLevel)
                            {
                                newPlayer.StrongestEnemy =
                                    new(battle.Enemy.InternalName, context.Interaction.Channel.Id);
                            }
                        }
                        else
                        {
                            newPlayer.StrongestEnemy = new(battle.Enemy.InternalName, context.Interaction.Channel.Id);
                        }

                        updatedPlayers.Add(newPlayer);
                    }

                    dbContext.UpdateRange(updatedPlayers);
                    await Task.WhenAll(relinquishChannelTask, dbContext.SaveChangesAsync());
                }

                await editTask;
            }
        }
        finally
        {
            foreach (DiscordPlayer playerInBattle in discordPlayers)
            {
                _ = liberationFeature.PlayersInBattles.Remove(playerInBattle.UserId, out bool _);
            }
        }
    }

    public override async Task OnButton(ButtonInteractionContext context)
    {
        // Really sad and awful linear search, but at least it works...?
        DiscordPlayer? clickingPlayer = null;
        PlayerState? clickingPlayerState = null;
        foreach ((IPlayer player, PlayerState playerState) in battle.Players)
        {
            if (player is DiscordPlayer discordPlayer && discordPlayer.UserId == context.Interaction.User.Id)
            {
                clickingPlayer = discordPlayer;
                clickingPlayerState = playerState;
            }
        }

        if (clickingPlayer is null || clickingPlayerState is null)
        {
            await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("You're not in this battle!")
                .WithFlags(MessageFlags.Ephemeral)));
            return;
        }

        if (clickingPlayerState.Health <= 0)
        {
            await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("You can't use an item right now...!")
                .WithFlags(MessageFlags.Ephemeral)));
            return;
        }

        if (context.Interaction.Data.CustomId.EndsWith("heal"))
        {
            if (clickingPlayer.Heal is null)
            {
                await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                    .WithContent("You don't have a healing item!")
                    .WithFlags(MessageFlags.Ephemeral)));
                return;
            }
            if (clickingPlayerState.Debuffs.Contains(Debuff.Sealed))
            {
                await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                    .WithContent("You can't use healing items while <:sealed:1426021766484201472> **Sealed!**")
                    .WithFlags(MessageFlags.Ephemeral)));
                return;
            }
            if (clickingPlayerState.HealUses > 0)
            {
                await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                    .WithContent("You've already used your healing item!")
                    .WithFlags(MessageFlags.Ephemeral)));
                return;
            }

            clickingPlayerState.QueuedIntervention = Intervention.UseHeal;
            await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("You'll use a **healing item** next turn.")
                .WithFlags(MessageFlags.Ephemeral)));
        }
        else if (context.Interaction.Data.CustomId.EndsWith("cure"))
        {
            if (clickingPlayer.Cure is not ICure cure)
            {
                await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                    .WithContent("You don't have a curative item!")
                    .WithFlags(MessageFlags.Ephemeral)));
                return;
            }
            if (clickingPlayerState.CureUses >= cure.GetMaxUses(clickingPlayer))
            {
                await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                    .WithContent("You've already used all of your curative item!")
                    .WithFlags(MessageFlags.Ephemeral)));
                return;
            }
            clickingPlayerState.QueuedIntervention = Intervention.UseCure;
            await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("You'll use a **curative item** next turn.")
                .WithFlags(MessageFlags.Ephemeral)));
        }
        else if (context.Interaction.Data.CustomId.EndsWith("ribbon"))
        {
            if (clickingPlayer.Ribbons <= 0)
            {
                await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                    .WithContent("You don't have any Ribbons!")
                    .WithFlags(MessageFlags.Ephemeral)));
                return;
            }
            if (clickingPlayerState.Debuffs.Contains(Debuff.Sealed))
            {
                await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                    .WithContent("You can't use Ribbons while <:sealed:1426021766484201472> **Sealed!**")
                    .WithFlags(MessageFlags.Ephemeral)));
                return;
            }
            if (clickingPlayerState.RibbonUses >= clickingPlayer.Ribbons)
            {
                await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                    .WithContent("You've used all of your Ribbons in this fight!")
                    .WithFlags(MessageFlags.Ephemeral)));
                return;
            }
            clickingPlayerState.QueuedIntervention = Intervention.UseRibbon;
            await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("You'll use a **Ribbon** next turn.")
                .WithFlags(MessageFlags.Ephemeral)));
        }
        else if (context.Interaction.Data.CustomId.EndsWith("no-item"))
        {
            clickingPlayerState.QueuedIntervention = Intervention.None;
            await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("You'll use **no item** next turn.")
                .WithFlags(MessageFlags.Ephemeral)));
        }
    }

    public string ProduceHealthBar(int healthPercent)
    {
        string healthTemplate;
        int healthMod;
        switch (healthPercent)
        {
            // First emoji
            case <= 33:
                healthTemplate = "{0}<:00:1424089155385819136><:e00:1424089155385819136>";
                healthMod = 0;
                break;
            case <= 66:
                healthTemplate = "<:100:1424089157256478791>{0}<:00:1424089155385819136>";
                healthMod = 33;
                break;
            default:
                healthTemplate = "<:100:1424089157256478791><:100:1424089157256478791>{0}";
                healthMod = 66;
                break;
        }

        int segmentHealth = healthPercent - healthMod;
        switch (segmentHealth)
        {
            case <= 8:
                return string.Format(healthTemplate, "<:00:1424089155385819136>");
            case <= 16:
                return string.Format(healthTemplate, "<:33:1429985155803648010>");
            case <= 24:
                return string.Format(healthTemplate, "<:66:1429985157686886530>");
            default:
                return string.Format(healthTemplate, "<:100:1424089157256478791>");
        }
    }
}
