using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetCord;

namespace Shardion.Terrabreak.Features.ShusoDivineReunion.Player;

public static class DiscordPlayerDbContextExtensions
{
    public static DiscordPlayer CreatePlayer(this DbContext context, ulong userId, string? name)
    {
        DiscordPlayer player = new()
        {
            UserId = userId,
            Name = name ?? "Kasane Teto",
        };
        context.Add(player);
        return player;
    }

    public static DiscordPlayer? GetPlayer(this DbContext context, ulong userId)
    {
        return context.Set<DiscordPlayer>().FirstOrDefault(player => player.UserId == userId);
    }

    public static async Task<DiscordPlayer> GetOrCreatePlayer(this DbContext context, ulong userId, string? name)
    {
        DiscordPlayer? existingPlayer = context.Set<DiscordPlayer>().FirstOrDefault(player => player.UserId == userId);
        if (existingPlayer is not null)
        {
            if (name is not null && existingPlayer.Name != name)
            {
                existingPlayer.Name = name;
                context.Update(existingPlayer);
                await context.SaveChangesAsync();
            }
            return existingPlayer;
        }

        DiscordPlayer newPlayer = CreatePlayer(context, userId, name);
        context.Add(newPlayer);
        await context.SaveChangesAsync();
        return newPlayer;
    }

    public static async Task<DiscordPlayer> GetOrCreatePlayerFromUserAsync(this DbContext context, User user)
    {
        DiscordPlayer? existingPlayer = context.Set<DiscordPlayer>().FirstOrDefault(player => player.UserId == user.Id);
        string userName = user.GlobalName ?? user.Username;
        if (user is GuildUser { Nickname: not null } guildUser)
        {
            userName = guildUser.Nickname;
        }
        if (existingPlayer is not null)
        {
            if (existingPlayer.Name != userName)
            {
                existingPlayer.Name = userName;
                context.Update(existingPlayer);
                await context.SaveChangesAsync();
            }
            return existingPlayer;
        }

        DiscordPlayer newPlayer = CreatePlayer(context, user.Id, userName);
        context.Add(newPlayer);
        await context.SaveChangesAsync();
        return newPlayer;
    }
}
