using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;
using Serilog;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;
using Shardion.Terrabreak.Features.ZannotGoodenough.Player;
using Shardion.Terrabreak.Features.ZannotGoodenough.Stadium;
using Shardion.Terrabreak.Services.Emoji;
using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.UserInterface;

public class InvestmentMenu(Battle battle, IdentityManager identity, EmojiManager emoji, MenuManager menu, TerrabreakMenu? returnTo) : TerrabreakMenu
{
    private BattleResolution? _resolution = null;
    private DateTimeOffset? _nextTurnTime = null;
    private readonly string _quote = Registries.BattleStartQuotes.Shuffle().First();

    public override async Task OnCreate(ApplicationCommandContext context, Guid guid)
    {
        battle.Start();
        await base.OnCreate(context, guid);
        _ = Task.Run(async () => await LoopToCompletion(context));
    }

    public override async Task OnReplace(IComponentInteractionContext context, Guid guid)
    {
        battle.Start();
        await base.OnReplace(context, guid);
        _ = Task.Run(async () => await LoopToCompletion(context));
    }

    public override Task<MenuMessage> BuildMessage()
    {
        List<IComponentContainerComponentProperties> components = [];

        // Reverse order, so Player 1's controls are on the same side as their monsters
        components.AddRange(battle.Player2.Player is DiscordPlayer
            ? ProduceEntriesForPlayer(battle.Player2, battle.Player1, InvestmentUiOwner.Player2)
            : ProduceEntriesForPlayer(battle.Player2, battle.Player1, InvestmentUiOwner.Computer));

        components.Add(new ComponentSeparatorProperties());

        if (_resolution is not null)
        {
            if (returnTo is not null && _resolution.Winner == battle.Player1)
            {
                components.Add(new ComponentSectionProperties(
                    new ButtonProperties($"menu:{MenuGuid}:continue", "Continue", ButtonStyle.Success),
                    [new TextDisplayProperties($"-# \u2044 \u2044  A winner is **{_resolution.Winner.Player.Name}**!!")]
                ));
            }
            else
            {
                components.Add(new TextDisplayProperties($"-# \u2044 \u2044  A winner is **{_resolution.Winner.Player.Name}**!!"));
            }
        }
        else if (_nextTurnTime is not null)
        {
            string lines = "";
            if (battle.Lines.Count > 0)
            {
                lines = "\n-# \u2044 \u2044  " + string.Join("\n-# \u2044 \u2044  ", battle.Lines);
            }
            components.Add(new TextDisplayProperties($"-# \u2044 \u2044  Advancing <t:{_nextTurnTime.Value.ToUnixTimeSeconds()}:R>{lines}"));
        }
        else
        {
            components.Add(new TextDisplayProperties($"-# \u2044 \u2044  {_quote}"));
        }

        components.Add(new ComponentSeparatorProperties());
        components.AddRange(ProduceEntriesForPlayer(battle.Player1, battle.Player2, InvestmentUiOwner.Player1));

        return Task.FromResult(new MenuMessage([
            new ComponentContainerProperties(components)
        ])
        {
            AllowedMentions = new()
            {
                AllowedUsers = [ ],
                AllowedRoles = [ ],
                Everyone = false,
            }
        });
    }

    private List<IComponentContainerComponentProperties> ProduceEntriesForPlayer(BattleLoadout player, BattleLoadout enemyPlayer, InvestmentUiOwner position)
    {
        List<IComponentContainerComponentProperties> components = [];
        StringBuilder monsters = new();
        foreach (BattleMonster monster in player.GetMonsterEnumerator())
        {
            string monsterKnockedOutMinimize = BattleRules.CheckKnockout(monster) ? "-# " : "";
            monsters.AppendLine($"{monsterKnockedOutMinimize}- {BattleMonster.ProduceClassificationIcon(emoji, monster)} {ProduceHealthBar(monster)} {monster.Monster.Name} {monster.State.CurrentHealth}/{monster.State.MaxHealth}");
        }

        components.Add(new TextDisplayProperties(monsters.ToString()));
        components.Add(new TextDisplayProperties($"-# \u2044 \u2044  {player.Player.GetMention()}\n{emoji.GetEmoji("like")} {player.Likes}"));

        if (position is not InvestmentUiOwner.Computer)
        {
            ActionRowProperties row = [];
            if (player.Monster1 is BattleMonster monster1)
            {
                LikesCostResult result = Battle.RunRules(new LikesCostRule(
                    new(this, null, monster1),
                    new(
                        battle,
                        player,
                        enemyPlayer,
                        monster1,
                        monster1.State,
                        monster1.Monster.LikesAbility.BaseLikesCost,
                        monster1.State.LikesCostStaticModifier,
                        monster1.State.LikesCostPercentageModifier
                    )));
                row.Add(new ButtonProperties($"menu:{MenuGuid}:{Enum.GetName(position)}:0", $"{monster1.Monster.LikesAbility.Name}", ButtonStyle.Secondary)
                {
                    Emoji = BattleMonster.ProduceClassificationIcon(emoji, monster1).ToProperties(),
                    Disabled = _resolution is not null || result.FinalLikesCost > player.Likes || BattleRules.CheckKnockout(monster1),
                });
            }
            if (player.Monster2 is BattleMonster monster2)
            {
                LikesCostResult result = Battle.RunRules(new LikesCostRule(
                    new(this, null, monster2),
                    new(
                        battle,
                        player,
                        enemyPlayer,
                        monster2,
                        monster2.State,
                        monster2.Monster.LikesAbility.BaseLikesCost,
                        monster2.State.LikesCostStaticModifier,
                        monster2.State.LikesCostPercentageModifier
                    )));
                row.Add(new ButtonProperties($"menu:{MenuGuid}:{Enum.GetName(position)}:1", $"{monster2.Monster.LikesAbility.Name}", ButtonStyle.Secondary)
                {
                    Emoji = BattleMonster.ProduceClassificationIcon(emoji, monster2).ToProperties(),
                    Disabled = _resolution is not null || result.FinalLikesCost > player.Likes || BattleRules.CheckKnockout(monster2),
                });
            }
            if (player.Monster3 is BattleMonster monster3)
            {
                LikesCostResult result = Battle.RunRules(new LikesCostRule(
                    new(this, null, monster3),
                    new(
                        battle,
                        player,
                        enemyPlayer,
                        monster3,
                        monster3.State,
                        monster3.Monster.LikesAbility.BaseLikesCost,
                        monster3.State.LikesCostStaticModifier,
                        monster3.State.LikesCostPercentageModifier
                    )));
                row.Add(new ButtonProperties($"menu:{MenuGuid}:{Enum.GetName(position)}:2", $"{monster3.Monster.LikesAbility.Name}", ButtonStyle.Secondary)
                {
                    Emoji = BattleMonster.ProduceClassificationIcon(emoji, monster3).ToProperties(),
                    Disabled = _resolution is not null || result.FinalLikesCost > player.Likes || BattleRules.CheckKnockout(monster3),
                });
            }
            components.Add(row);
        }

        if (position is not InvestmentUiOwner.Player1)
        {
            components.Reverse();
        }

        return components;
    }

    private string ProduceHealthBar(BattleMonster monster)
    {
        if (monster.State.MaxHealth <= 0)
        {
            // Hard-coding this case prevents divide by zero
            return $"{emoji.GetEmoji("hpl0")}{emoji.GetEmoji("hp0")}{emoji.GetEmoji("hp0")}{emoji.GetEmoji("hpr0")}";
        }

        double healthFraction = (double)monster.State.CurrentHealth / monster.State.MaxHealth;
        return healthFraction switch
        {
            // empty
            <= 0.00 => $"{emoji.GetEmoji("hpl0")}{emoji.GetEmoji("hp0")}{emoji.GetEmoji("hp0")}{emoji.GetEmoji("hpr0")}",

            <= 0.11 => $"{emoji.GetEmoji("hpl1")}{emoji.GetEmoji("hp0")}{emoji.GetEmoji("hp0")}{emoji.GetEmoji("hpr0")}",
            <= 0.22 => $"{emoji.GetEmoji("hpl2")}{emoji.GetEmoji("hpcl1")}{emoji.GetEmoji("hp0")}{emoji.GetEmoji("hpr0")}",
            <= 0.33 => $"{emoji.GetEmoji("hpl3")}{emoji.GetEmoji("hpcl2")}{emoji.GetEmoji("hp0")}{emoji.GetEmoji("hpr0")}",
            <= 0.44 => $"{emoji.GetEmoji("hpl3")}{emoji.GetEmoji("hpcl3")}{emoji.GetEmoji("hpcr1")}{emoji.GetEmoji("hpr0")}",
            <= 0.55 => $"{emoji.GetEmoji("hpl3")}{emoji.GetEmoji("hpcl4")}{emoji.GetEmoji("hpcr2")}{emoji.GetEmoji("hpr0")}",
            <= 0.66 => $"{emoji.GetEmoji("hpl3")}{emoji.GetEmoji("hp100")}{emoji.GetEmoji("hpcr3")}{emoji.GetEmoji("hpr0")}",
            <= 0.77 => $"{emoji.GetEmoji("hpl3")}{emoji.GetEmoji("hp100")}{emoji.GetEmoji("hpcr4")}{emoji.GetEmoji("hpr1")}",
            <= 0.88 => $"{emoji.GetEmoji("hpl3")}{emoji.GetEmoji("hp100")}{emoji.GetEmoji("hp100")}{emoji.GetEmoji("hpr2")}",

            // full
            _ => $"{emoji.GetEmoji("hpl3")}{emoji.GetEmoji("hp100")}{emoji.GetEmoji("hp100")}{emoji.GetEmoji("hpr3")}",
        };
    }

    public override async Task OnButton(ButtonInteractionContext context)
    {
        string[] splitCustomId = context.Interaction.Data.CustomId.Split(':');
        if (splitCustomId.Length < 4)
        {
            if (splitCustomId.Last() == "continue" && returnTo is not null && _resolution is not null)
            {
                if (_resolution.Winner.Player is DiscordPlayer discordPlayer &&
                    context.User.Id != discordPlayer.DiscordUserId)
                {
                    await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                        .WithContent(identity.GetAccessDeniedResponse())
                        .WithFlags(MessageFlags.Ephemeral)
                    ));
                }
                else
                {
                    await ReplaceMenuAsync(context, menu, returnTo);
                }
            }
            return;
        }

        InvestmentUiOwner position = Enum.Parse<InvestmentUiOwner>(splitCustomId[2]);
        int monsterIndex = int.Parse(splitCustomId[3]);

        BattleLoadout playerLoadout = position switch
        {
            InvestmentUiOwner.Player1 => battle.Player1,
            InvestmentUiOwner.Player2 => battle.Player2,
            _ => throw new ArgumentOutOfRangeException(nameof(position), position, null)
        };

        if (playerLoadout.Player is not DiscordPlayer player)
        {
            throw new InvalidOperationException("Cannot use the UI to control a computer player!");
        }

        if (player.DiscordUserId != context.User.Id)
        {
            await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent(identity.GetAccessDeniedResponse())
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }

        BattleMonster? monster = monsterIndex switch
        {
            0 => playerLoadout.Monster1,
            1 => playerLoadout.Monster2,
            2 => playerLoadout.Monster3,
            _ => throw new ArgumentOutOfRangeException(nameof(position), position, null)
        };

        if (monster is not null)
        {
            if (position == InvestmentUiOwner.Player1)
            {
                battle.Player1Intent = new(monster);
            }
            else
            {
                battle.Player2Intent = new(monster);
            }
        }

        MenuMessage message = await BuildMessage();
        await RespondAsync(context, InteractionCallback.ModifyMessage(responseMessage => responseMessage
            .WithAttachments(message.Attachments)
            .WithComponents(message.Components)
            .WithFlags(message.Flags | MessageFlags.IsComponentsV2)
            .WithAllowedMentions(message.AllowedMentions)));
    }

    public async Task LoopToCompletion(IInteractionContext context)
    {
        // TODO: `while` is scary...
        try
        {
            while (_resolution is null)
            {
                // Manually stave off menu GC
                LastInteractionTime = DateTimeOffset.UtcNow;

                // Sleep 5 seconds, but thread-safe
                await Task.Delay(TimeSpan.FromSeconds(8));

                // Despite turns only taking 5 seconds, we display 6 seconds, to account for response timings
                _nextTurnTime = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(9);
                _resolution = battle.Turn();

                MenuMessage message = await BuildMessage();
                await ModifyResponseAsync(context, responseMessage => responseMessage
                    .WithAttachments(message.Attachments)
                    .WithComponents(message.Components)
                    .WithFlags(message.Flags | MessageFlags.IsComponentsV2)
                    .WithAllowedMentions(message.AllowedMentions));
            }
        }
        catch (Exception e)
        {
            Log.Error("Battle failed with exception!! {e}", e);
            MenuMessage message = BuildError(e);
            await ModifyResponseAsync(context, responseMessage => responseMessage
                .WithAttachments(message.Attachments)
                .WithComponents(message.Components)
                .WithFlags(message.Flags | MessageFlags.IsComponentsV2)
                .WithAllowedMentions(message.AllowedMentions));
        }
    }

    private MenuMessage BuildError(Exception e)
    {
        byte[] utf16ExceptionDetails = Encoding.Default.GetBytes(e.ToString());
        byte[] utf8ExceptionDetails = Encoding.Convert(Encoding.Default, Encoding.UTF8, utf16ExceptionDetails);
        AttachmentProperties exceptionDetailsAttachment = new("exception.txt", new MemoryStream(utf8ExceptionDetails));
        StringBuilder instanceOwnerMentions = new("");
        foreach (ulong instanceOwner in identity.Options.InstanceOwnerIds)
        {
            instanceOwnerMentions.Append($" <@{instanceOwner.ToString(CultureInfo.InvariantCulture)}>");
        }

        List<IComponentContainerComponentProperties> components =
        [
            new TextDisplayProperties("### Error\nAn error has occurred and this command has been halted."),
            new TextDisplayProperties($"> `{e.GetType().Name}`: {e.Message}"),
            new FileDisplayProperties(new("attachment://exception.txt")),
            new TextDisplayProperties($"-# {identity.Options.BotName} error!! There's an unhandled error! Humans! Fix it!{instanceOwnerMentions}")
        ];

        return new([new ComponentContainerProperties(components)])
        {
            AllowedMentions = new()
            {
                AllowedUsers = identity.Options.InstanceOwnerIds,
            },
            Attachments = [exceptionDetailsAttachment]
        };
    }
}
