using System.Collections.Generic;
using System.Text;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;
using Shardion.Terrabreak.Features.ZannotGoodenough.Player;
using Shardion.Terrabreak.Features.ZannotGoodenough.Relics;
using Shardion.Terrabreak.Services.Emoji;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;

public class BattleLoadout
{
    public IPlayer Player { get; init; }
    public int Likes { get; set; }

    public BattleRelic? Relic1 { get; set; }
    public BattleRelic? Relic2 { get; set; }
    public BattleRelic? Relic3 { get; set; }
    public BattleRelic? Relic4 { get; set; }

    public BattleRelic?[] Relics => [Relic1, Relic2, Relic3, Relic4];

    public IEnumerable<BattleRelic> GetRelicEnumerator()
    {
        if (Relic1 is not null)
        {
            yield return Relic1;
        }
        if (Relic2 is not null)
        {
            yield return Relic2;
        }
        if (Relic3 is not null)
        {
            yield return Relic3;
        }
        if (Relic4 is not null)
        {
            yield return Relic4;
        }
    }

    public IEnumerable<string> GetRelicIdentifierEnumerator()
    {
        if (Relic1 is not null)
        {
            yield return Relic1.Relic.InternalName;
        }
        if (Relic2 is not null)
        {
            yield return Relic2.Relic.InternalName;
        }
        if (Relic3 is not null)
        {
            yield return Relic3.Relic.InternalName;
        }
        if (Relic4 is not null)
        {
            yield return Relic4.Relic.InternalName;
        }
    }

    public BattleMonster? Monster1 { get; set; }
    public BattleMonster? Monster2 { get; set; }
    public BattleMonster? Monster3 { get; set; }

    public BattleMonster?[] Monsters => [Monster1, Monster2, Monster3];

    public IEnumerable<BattleMonster> GetMonsterEnumerator()
    {
        if (Monster1 is not null)
        {
            yield return Monster1;
        }
        if (Monster2 is not null)
        {
            yield return Monster2;
        }
        if (Monster3 is not null)
        {
            yield return Monster3;
        }
    }

    public void Reset()
    {
        Likes = 0;

        if (Monster1 is not null)
        {
            Monster1 = new(Monster1.Monster);
        }
        if (Monster2 is not null)
        {
            Monster2 = new(Monster2.Monster);
        }
        if (Monster3 is not null)
        {
            Monster3 = new(Monster3.Monster);
        }

        if (Relic1 is not null)
        {
            Relic1 = new(Relic1.Relic);
        }
        if (Relic2 is not null)
        {
            Relic2 = new(Relic2.Relic);
        }
        if (Relic3 is not null)
        {
            Relic3 = new(Relic3.Relic);
        }
        if (Relic4 is not null)
        {
            Relic4 = new(Relic4.Relic);
        }
    }

    public static string ProduceLoadoutLine(EmojiManager emoji, BattleLoadout loadout)
    {
        StringBuilder b = new();
        foreach (BattleMonster? monster in loadout.Monsters)
        {
            if (monster is not null)
            {
                b.Append(BattleMonster.ProduceClassificationIcon(emoji, monster));
            }
            else
            {
                b.Append(emoji.GetEmoji("noitem"));
            }
        }
        b.Append("   ⁄ ⁄   ");
        foreach (BattleRelic? relic in loadout.Relics)
        {
            if (relic is not null)
            {
                b.Append(emoji.GetEmoji(relic.Relic.Series.EmojiIdentifier));
            }
            else
            {
                b.Append(emoji.GetEmoji("noitem"));
            }
        }
        return b.ToString();
    }

    public static BattleLoadout FromLoadout(Loadout loadout, IPlayer player)
    {
        BattleMonster? monster1 = loadout.Monster1Identifier is string monster1Id
            ? new(Registries.Monsters.Forward[monster1Id])
            : null;
        BattleMonster? monster2 = loadout.Monster2Identifier is string monster2Id
            ? new(Registries.Monsters.Forward[monster2Id])
            : null;
        BattleMonster? monster3 = loadout.Monster3Identifier is string monster3Id
            ? new(Registries.Monsters.Forward[monster3Id])
            : null;

        BattleRelic? relic1 = loadout.Relic1Identifier is string relic1Id
            ? new(Registries.Relics.Forward[relic1Id])
            : null;
        BattleRelic? relic2 = loadout.Relic2Identifier is string relic2Id
            ? new(Registries.Relics.Forward[relic2Id])
            : null;
        BattleRelic? relic3 = loadout.Relic3Identifier is string relic3Id
            ? new(Registries.Relics.Forward[relic3Id])
            : null;
        BattleRelic? relic4 = loadout.Relic4Identifier is string relic4Id
            ? new(Registries.Relics.Forward[relic4Id])
            : null;

        return new()
        {
            Player = player,
            Monster1 = monster1,
            Monster2 = monster2,
            Monster3 = monster3,
            Relic1 = relic1,
            Relic2 = relic2,
            Relic3 = relic3,
            Relic4 = relic4,
        };
    }
}
