using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;
using Shardion.Terrabreak.Features.ZannotGoodenough.Relics;
using Shardion.Terrabreak.Services.Emoji;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Player;

[Owned]
public class Loadout
{
    public string? Relic1Identifier { get; set; }
    public string? Relic2Identifier { get; set; }
    public string? Relic3Identifier { get; set; }
    public string? Relic4Identifier { get; set; }
    public string? Relic5Identifier { get; set; }
    public string? Relic6Identifier { get; set; }

    [NotMapped]
    public string?[] RelicIdentifiers =>
    [
        Relic1Identifier, Relic2Identifier, Relic3Identifier, Relic4Identifier, Relic5Identifier, Relic6Identifier
    ];

    public string? Monster1Identifier { get; set; }
    public string? Monster2Identifier { get; set; }
    public string? Monster3Identifier { get; set; }

    [NotMapped]
    public string?[] MonsterIdentifiers =>
    [
        Monster1Identifier, Monster2Identifier, Monster3Identifier,
    ];

    public IEnumerable<string> GetRelicIdentifierEnumerator()
    {
        if (Relic1Identifier is not null)
        {
            yield return Relic1Identifier;
        }
        if (Relic2Identifier is not null)
        {
            yield return Relic2Identifier;
        }
        if (Relic3Identifier is not null)
        {
            yield return Relic3Identifier;
        }
        if (Relic4Identifier is not null)
        {
            yield return Relic4Identifier;
        }
        if (Relic5Identifier is not null)
        {
            yield return Relic5Identifier;
        }
        if (Relic6Identifier is not null)
        {
            yield return Relic6Identifier;
        }
    }

    public IEnumerable<IRelic<RelicState>> GetRelicEnumerator()
    {
        if (Relic1Identifier is not null)
        {
            yield return Registries.Relics.Forward[Relic1Identifier];
        }
        if (Relic2Identifier is not null)
        {
            yield return Registries.Relics.Forward[Relic2Identifier];
        }
        if (Relic3Identifier is not null)
        {
            yield return Registries.Relics.Forward[Relic3Identifier];
        }
        if (Relic4Identifier is not null)
        {
            yield return Registries.Relics.Forward[Relic4Identifier];
        }
        if (Relic5Identifier is not null)
        {
            yield return Registries.Relics.Forward[Relic5Identifier];
        }
        if (Relic6Identifier is not null)
        {
            yield return Registries.Relics.Forward[Relic6Identifier];
        }
    }


    public IEnumerable<string> GetMonsterIdentifierEnumerator()
    {
        if (Monster1Identifier is not null)
        {
            yield return Monster1Identifier;
        }
        if (Monster2Identifier is not null)
        {
            yield return Monster2Identifier;
        }
        if (Monster3Identifier is not null)
        {
            yield return Monster3Identifier;
        }
    }

    public IEnumerable<IMonster<MonsterState>> GetMonsterEnumerator()
    {
        if (Monster1Identifier is not null)
        {
            yield return Registries.Monsters.Forward[Monster1Identifier];
        }
        if (Monster2Identifier is not null)
        {
            yield return Registries.Monsters.Forward[Monster2Identifier];
        }
        if (Monster3Identifier is not null)
        {
            yield return Registries.Monsters.Forward[Monster3Identifier];
        }
    }

    public static string ProduceLoadoutLine(EmojiManager emoji, Loadout loadout)
    {
        StringBuilder b = new();
        foreach (string? monsterId in loadout.MonsterIdentifiers)
        {
            if (monsterId is not null)
            {
                IMonster<MonsterState> monster = Registries.Monsters.Forward[monsterId];
                b.Append(IMonster<MonsterState>.ProduceClassificationIcon(emoji, monster));
            }
            else
            {
                b.Append(emoji.GetEmoji("noitem"));
            }
        }
        b.Append("   ⁄ ⁄   ");
        foreach (string? relicId in loadout.RelicIdentifiers)
        {
            if (relicId is not null)
            {
                IRelic<RelicState> relic = Registries.Relics.Forward[relicId];
                b.Append(emoji.GetEmoji(relic.Series.EmojiIdentifier));
            }
            else
            {
                b.Append(emoji.GetEmoji("noitem"));
            }
        }
        return b.ToString();
    }
}
