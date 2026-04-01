using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Player;

[Index(nameof(DiscordUserId))]
public class DiscordPlayer : IPlayer
{
    [Key]
    public Guid Id { get; init; } = Guid.NewGuid();

    public required ulong DiscordUserId { get; init; }

    // Not mapped due to being produced from Discord data on demand, since nicknames can change often
    [NotMapped] public string Name { get; set; } = "Kasane Teto";

    public int EquippedLoadoutIndex { get; set; }

    [NotMapped]
    public Loadout EquippedLoadout
    {
        get
        {
            return EquippedLoadoutIndex switch
            {
                0 => Loadout1,
                1 => Loadout2,
                2 => Loadout3,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public required Loadout Loadout1 { get; set; }
    public required Loadout Loadout2 { get; set; }
    public required Loadout Loadout3 { get; set; }

    // TODO: Make multiple adds to this not a footgun. I would have used ISet<>, but EF Core doesn't support it...
    public IList<string> UnlockedRelicIdentifiers { get; set; } = [];
    public IList<string> UnlockedMonsterIdentifiers { get; set; } = [];

    public string GetMention()
    {
        return $"<@{DiscordUserId}>";
    }
}
