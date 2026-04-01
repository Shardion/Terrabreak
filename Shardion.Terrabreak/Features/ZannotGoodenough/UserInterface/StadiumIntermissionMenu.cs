using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Rodent;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Spirit;
using Shardion.Terrabreak.Features.ZannotGoodenough.Player;
using Shardion.Terrabreak.Features.ZannotGoodenough.Stadium;
using Shardion.Terrabreak.Services.Database;
using Shardion.Terrabreak.Services.Emoji;
using Shardion.Terrabreak.Services.Identity;
using Shardion.Terrabreak.Services.Menuing;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.UserInterface;

public class StadiumIntermissionMenu : TerrabreakMenu
{
    private readonly IDbContextFactory<TerrabreakDatabaseContext> _dbContextFactory;
    private readonly IdentityManager _identity;
    private readonly EmojiManager _emoji;
    private readonly MenuManager _menu;
    private readonly DiscordPlayer _player;
    private readonly StadiumContext _stadiumContext;
    private readonly BattleLoadout _opponent;

    public BattleLoadout Loadout { get; }

    public StadiumIntermissionMenu(IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory,
        IdentityManager identity, EmojiManager emoji, MenuManager menu, DiscordPlayer player,
        StadiumContext stadiumContext)
    {
        _dbContextFactory = dbContextFactory;
        _identity = identity;
        _emoji = emoji;
        _menu = menu;
        _player = player;
        _stadiumContext = stadiumContext;

        _opponent = _stadiumContext.GenerateOpponent();

        BattleMonster? monster1 = _player.EquippedLoadout.Monster1Identifier is string monster1Id
            ? new(Registries.Monsters.Forward[monster1Id])
            : null;
        BattleMonster? monster2 = _player.EquippedLoadout.Monster2Identifier is string monster2Id
            ? new(Registries.Monsters.Forward[monster2Id])
            : null;
        BattleMonster? monster3 = _player.EquippedLoadout.Monster3Identifier is string monster3Id
            ? new(Registries.Monsters.Forward[monster3Id])
            : null;

        BattleRelic? relic1 = _player.EquippedLoadout.Relic1Identifier is string relic1Id
            ? new(Registries.Relics.Forward[relic1Id])
            : null;
        BattleRelic? relic2 = _player.EquippedLoadout.Relic2Identifier is string relic2Id
            ? new(Registries.Relics.Forward[relic2Id])
            : null;
        BattleRelic? relic3 = _player.EquippedLoadout.Relic3Identifier is string relic3Id
            ? new(Registries.Relics.Forward[relic3Id])
            : null;
        BattleRelic? relic4 = _player.EquippedLoadout.Relic4Identifier is string relic4Id
            ? new(Registries.Relics.Forward[relic4Id])
            : null;

        Loadout = new()
        {
            Player = _player,
            Monster1 = monster1,
            Monster2 = monster2,
            Monster3 = monster3,
            Relic1 = relic1,
            Relic2 = relic2,
            Relic3 = relic3,
            Relic4 = relic4
        };
    }

    public StadiumIntermissionMenu(IDbContextFactory<TerrabreakDatabaseContext> dbContextFactory, IdentityManager identity, EmojiManager emoji, MenuManager menu, DiscordPlayer player, StadiumContext stadiumContext, BattleLoadout loadout)
    {
        _dbContextFactory = dbContextFactory;
        _identity = identity;
        _emoji = emoji;
        _menu = menu;
        _player = player;
        _stadiumContext = stadiumContext;

        _opponent = _stadiumContext.GenerateOpponent();
        Loadout = loadout;
        Loadout.Reset();
    }

    public override Task OnCreate(ApplicationCommandContext context, Guid guid)
    {
        Loadout.Reset();
        return base.OnCreate(context, guid);
    }

    public override Task OnReplace(IComponentInteractionContext context, Guid guid)
    {
        Loadout.Reset();
        return base.OnReplace(context, guid);
    }

    public override Task<MenuMessage> BuildMessage()
    {
        string playerLoadoutLine = BattleLoadout.ProduceLoadoutLine(_emoji, Loadout);
        string opponentLoadoutLine = BattleLoadout.ProduceLoadoutLine(_emoji, _opponent);

        List<IComponentContainerComponentProperties> components = [
            new TextDisplayProperties($"### Round {_stadiumContext.CurrentRound + 1}"),
            new TextDisplayProperties($"{_opponent.Player.Name}'s loadout:\n- {opponentLoadoutLine}"),
            new TextDisplayProperties($"Your loadout:\n- {playerLoadoutLine}"),
            new ActionRowProperties([
                new ButtonProperties($"menu:{MenuGuid}:continue", "Fight!", ButtonStyle.Primary),
                new ButtonProperties($"menu:{MenuGuid}:loadout", "Swap Relics", ButtonStyle.Secondary),
            ])
        ];

        return Task.FromResult(new MenuMessage([new ComponentContainerProperties(components)]));
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
        string[] splitCustomId = context.Interaction.Data.CustomId.Split(':');
        if (splitCustomId.Last() == "continue")
        {
            TerrabreakMenu nextIntermission;
            if (_stadiumContext.CurrentRound >= _stadiumContext.TotalRounds - 1)
            {
                nextIntermission = new StadiumVictoryMenu(_dbContextFactory,
                    _identity,
                    _emoji,
                    _menu,
                    _player,
                    _stadiumContext with
                    {
                        CurrentRound = _stadiumContext.CurrentRound + 1,
                    });
            }
            else
            {
                nextIntermission = new StadiumIntermissionMenu(_dbContextFactory,
                    _identity,
                    _emoji,
                    _menu,
                    _player,
                    _stadiumContext with
                    {
                        CurrentRound = _stadiumContext.CurrentRound + 1,
                    },
                    Loadout);
            }
            await ReplaceMenuAsync(context, _menu, new InvestmentMenu(new()
                {
                    Player1 = Loadout,
                    Player2 = _opponent,
                }, _identity, _emoji, _menu, nextIntermission));
        }
        else if (splitCustomId.Last() == "loadout")
        {
            await ReplaceMenuAsync(context, _menu, new EditBattleLoadoutMenu(_dbContextFactory, _identity, _emoji, _menu, _player, Loadout, this));
        }
    }
}
