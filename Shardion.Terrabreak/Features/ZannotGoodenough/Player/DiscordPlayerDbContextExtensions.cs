using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Rest;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;
using Shardion.Terrabreak.Features.ZannotGoodenough.Relics;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Player;

public static class DiscordPlayerDbContextExtensions
{
    public static DiscordPlayer CreateInactivePlayer(this DbContext dbContext, ZannotGoodenoughOptions options, ulong userId)
    {
        string? userName = null;
        if (userId == options.PresidentZannotUserId)
        {
            userName = "President Zannot";
        }
        DiscordPlayer player = GeneratePlayerObject(options, userId, userName);
        dbContext.Add(player);
        return player;
    }

    public static DiscordPlayer? GetInactivePlayer(this DbContext dbContext, ZannotGoodenoughOptions options, ulong userId)
    {
        DiscordPlayer? player = dbContext.Set<DiscordPlayer>().FirstOrDefault(player => player.DiscordUserId == userId);
        if (player is not null && userId == options.PresidentZannotUserId)
        {
            player.Name = "President Zannot";
        }
        return player;
    }

    public static async Task<DiscordPlayer> GetOrCreateInactivePlayerAsync(this DbContext dbContext, ZannotGoodenoughOptions options, ulong userId, bool saveOnCreate = true)
    {
        DiscordPlayer? existingPlayer = dbContext.Set<DiscordPlayer>().FirstOrDefault(player => player.DiscordUserId == userId);
        if (existingPlayer is not null)
        {
            if (userId == options.PresidentZannotUserId)
            {
                existingPlayer.Name = "President Zannot";
            }
            return existingPlayer;
        }

        DiscordPlayer newPlayer = dbContext.CreateInactivePlayer(options, userId);
        dbContext.Add(newPlayer);
        if (saveOnCreate)
        {
            await dbContext.SaveChangesAsync();
        }

        return newPlayer;
    }

    public static DiscordPlayer CreatePlayer(this DbContext dbContext, ZannotGoodenoughOptions options, User user)
    {
        string userName;
        if (user.Id == options.PresidentZannotUserId)
        {
            userName = "President Zannot";
        }
        else if (user is GuildUser { Nickname: not null } guildUser)
        {
            userName = guildUser.Nickname;
        }
        else
        {
            userName = user.GlobalName ?? user.Username;
        }

        DiscordPlayer player = GeneratePlayerObject(options, user.Id, userName);
        dbContext.Add(player);
        return player;
    }

    public static DiscordPlayer? GetPlayer(this DbContext dbContext, ZannotGoodenoughOptions options, User user)
    {
        DiscordPlayer? player = dbContext.Set<DiscordPlayer>().FirstOrDefault(player => player.DiscordUserId == user.Id);
        if (player is not null)
        {
            if (user.Id == options.PresidentZannotUserId)
            {
                player.Name = "President Zannot";
            }
            else if (user is GuildUser { Nickname: not null } guildUser)
            {
                player.Name = guildUser.Nickname;
            }
            else
            {
                player.Name = user.GlobalName ?? user.Username;
            }
        }

        return player;
    }

    public static async Task<DiscordPlayer> GetOrCreatePlayerAsync(this DbContext dbContext, ZannotGoodenoughOptions options, User user, bool saveOnCreate = true)
    {
        DiscordPlayer? existingPlayer = dbContext.GetPlayer(options, user);
        if (existingPlayer is not null)
        {
            return existingPlayer;
        }

        DiscordPlayer newPlayer = dbContext.CreatePlayer(options, user);
        if (saveOnCreate)
        {
            await dbContext.SaveChangesAsync();
        }
        return newPlayer;
    }

    public static bool UnlockMonsterForPlayer(this DbContext dbContext, DiscordPlayer player, IMonster<MonsterState> monster)
    {
        if (player.UnlockedMonsterIdentifiers.Contains(monster.InternalName))
        {
            return false;
        }
        player.UnlockedMonsterIdentifiers.Add(monster.InternalName);
        dbContext.Update(player);
        return true;
    }

    public static bool UnlockRelicForPlayer(this DbContext dbContext, DiscordPlayer player, IRelic<RelicState> relic)
    {
        if (player.UnlockedRelicIdentifiers.Contains(relic.InternalName))
        {
            return false;
        }
        player.UnlockedRelicIdentifiers.Add(relic.InternalName);
        dbContext.Update(player);
        return true;
    }

    private static DiscordPlayer GeneratePlayerObject(ZannotGoodenoughOptions options, ulong uid, string? name)
    {
        IRelicSeries investorCore = Registries.RelicSeries.Forward["InvestorCore"];
        IEnumerable<IRelic<RelicState>> investorCoreRelics = Registries.Relics.Contents.Where(relic => relic.Series == investorCore);

        DiscordPlayer player;
        if (options.PresidentZannotUserId == uid)
        {
            IRelicSeries tricksOfTheTrade = Registries.RelicSeries.Forward["TricksOfTheTrade"];
            IEnumerable<IRelic<RelicState>> tricksOfTheTradeRelics = Registries.Relics.Contents.Where(relic => relic.Series == tricksOfTheTrade);
            List<string> startingRelics = investorCoreRelics.Select(relic => relic.InternalName).ToList();
            startingRelics.AddRange(tricksOfTheTradeRelics.Select(relic => relic.InternalName));

            player = new()
            {
                DiscordUserId = uid,
                UnlockedMonsterIdentifiers = ["RetRat", "RazRat", "BroccoliMan"],
                UnlockedRelicIdentifiers = startingRelics,
                Loadout1 = new()
                {
                    Monster1Identifier = "RetRat",
                    Monster2Identifier = "RetRat",
                    Monster3Identifier = "RetRat",
                },
                Loadout2 = new(),
                Loadout3 = new(),
            };
        }
        else
        {
            player = new()
            {
                DiscordUserId = uid,
                UnlockedMonsterIdentifiers = ["RetRat", "RazRat", "BroccoliMan"],
                UnlockedRelicIdentifiers = investorCoreRelics.Select(relic => relic.InternalName).ToList(),
                Loadout1 = new()
                {
                    Monster1Identifier = "RetRat",
                    Monster2Identifier = "RetRat",
                    Monster3Identifier = "RetRat",
                },
                Loadout2 = new(),
                Loadout3 = new(),
            };
        }

        if (name is not null)
        {
            player.Name = name;
        }

        return player;
    }
}
