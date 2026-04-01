using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;
using Shardion.Terrabreak.Features.ZannotGoodenough.Player;
using Shardion.Terrabreak.Features.ZannotGoodenough.Relics;
using Shardion.Terrabreak.Features.ZannotGoodenough.Stadium;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Emoji;
using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.UserInterface;

public class StadiumVictoryMenu : TerrabreakMenu
{
    private readonly IDbContextFactory<TerrabreakDatabaseContext> _dbContextFactory;
    private readonly IdentityManager _identity;
    private readonly EmojiManager _emoji;
    private readonly DiscordPlayer _player;
    private readonly StadiumContext _context;

    public StadiumVictoryMenu(IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory, IdentityManager identity, EmojiManager emoji, MenuManager menu, DiscordPlayer player, StadiumContext context)
    {
        _dbContextFactory = dbContextFactory;
        _identity = identity;
        _emoji = emoji;
        _player = player;
        _context = context;
        GenerateRewards();
    }

    public IMonster<MonsterState>? WonMonster { get; set; }
    public IRelic<RelicState>? WonRelic { get; set; }
    public bool Claimed { get; set; }

    public override Task<MenuMessage> BuildMessage()
    {
        List<IComponentContainerComponentProperties> components = [
            new TextDisplayProperties("### Victory!"),
            new TextDisplayProperties($"You've successfully cleared{(_context.TotalRounds > 1 ? " all" : "")} **{_context.TotalRounds} round{(_context.TotalRounds > 1 ? "s" : "")}!**"),
        ];

        if (WonRelic is not null || WonMonster is not null)
        {
            StringBuilder winningsTextBuilder = new("Earned:\n");
            if (WonRelic is not null)
            {
                winningsTextBuilder.AppendLine($"- {_emoji.GetEmoji(WonRelic.Series.EmojiIdentifier)} {WonRelic.Name}");
            }
            if (WonMonster is not null)
            {
                winningsTextBuilder.AppendLine($"- {IMonster<MonsterState>.ProduceClassificationIcon(_emoji, WonMonster).ToString()} {WonMonster.Name}");
            }
            components.Add(new TextDisplayProperties(winningsTextBuilder.ToString()));
            if (!Claimed)
            {
                components.Add(new ActionRowProperties([
                    new ButtonProperties($"menu:{MenuGuid}:claim", "Claim Rewards", ButtonStyle.Success)
                    {
                        Disabled = false,
                    }
                ]));
            }
            else
            {
                components.Add(new ActionRowProperties([
                    new ButtonProperties($"menu:{MenuGuid}:claim", "Claimed!", ButtonStyle.Success)
                    {
                        Disabled = true,
                    }
                ]));
            }
        }

        return Task.FromResult(new MenuMessage([new ComponentContainerProperties(components)]));
    }

    private void GenerateRewards()
    {
        if (Claimed)
        {
            return;
        }

        double monsterRng = Random.Shared.NextDouble();
        double monsterRewardRequirement = _context.TotalRounds switch
        {
            >= 10 => 1.0,
            >= 7 => 0.90,
            >= 5 => 0.75,
            >= 3 => 0.50,
            >= 0 => 0.33,
            _ => throw new ArgumentOutOfRangeException()
        };
        if (monsterRng <= monsterRewardRequirement)
        {
            WonMonster = Registries.Monsters.Contents
                .Where(monster => !monster.Hidden && !_player.UnlockedMonsterIdentifiers.Contains(monster.InternalName))
                .Shuffle()
                .FirstOrDefault();
        }

        double relicRng = Random.Shared.NextDouble();
        double tierThreeRewardRequirement = _context.TotalRounds switch
        {
            >= 10 => 0.95,
            >= 7 => 0.75,
            >= 5 => 0.50,
            >= 3 => 0.25,
            >= 0 => 0.05,
            _ => throw new ArgumentOutOfRangeException()
        };
        if (relicRng <= tierThreeRewardRequirement)
        {
            WonRelic = Registries.Relics.Contents
                .Where(relic => relic.Series.Tier == RelicTier.TierThree && !_player.UnlockedRelicIdentifiers.Contains(relic.InternalName))
                .Shuffle()
                .FirstOrDefault();
        }
        if (WonRelic is null)
        {
            WonRelic = Registries.Relics.Contents
                .Where(relic => relic.Series.Tier == RelicTier.TierTwo && !_player.UnlockedRelicIdentifiers.Contains(relic.InternalName))
                .Shuffle()
                .FirstOrDefault();
        }
    }

    public override async Task OnButton(ButtonInteractionContext context)
    {
        if (_player.DiscordUserId != context.User.Id)
        {
            await RespondAsync(context, InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent(_identity.GetAccessDeniedResponse())
                .WithFlags(MessageFlags.Ephemeral)
            ));
            return;
        }
        TerrabreakDatabaseContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        if (WonMonster is not null)
        {
            dbContext.UnlockMonsterForPlayer(_player, WonMonster);
        }

        if (WonRelic is not null)
        {
            dbContext.UnlockRelicForPlayer(_player, WonRelic);
        }

        Claimed = true;

        MenuMessage message = await BuildMessage();
        Task respondTask = RespondAsync(context, InteractionCallback.ModifyMessage(responseMessage => responseMessage
            .WithAttachments(message.Attachments)
            .WithComponents(message.Components)
            .WithFlags(message.Flags | MessageFlags.IsComponentsV2)
            .WithAllowedMentions(message.AllowedMentions)));

        await Task.WhenAll(
            dbContext.SaveChangesAsync(),
            respondTask
        );
    }
}
