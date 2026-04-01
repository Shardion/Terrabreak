using System;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;
using Shardion.Terrabreak.Services.Emoji;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;

public record BattleMonster
{
    public IMonster<MonsterState> Monster { get; }
    public MonsterState State { get; }

    public BattleMonster(IMonster<MonsterState> monster)
    {
        Monster = monster;
        State = monster.NewState();
        State.MaxHealth = monster.BaseHealth;
        State.CurrentHealth = monster.BaseHealth;
    }

    public static ManagedEmoji ProduceClassificationIcon(EmojiManager emojiManager, BattleMonster monster)
    {
        if (BattleRules.CheckKnockout(monster))
        {
            return monster.Monster.Classification switch
            {
                MonsterClassification.Rodent => emojiManager.GetEmoji("rodentinactive"),
                MonsterClassification.Nature => emojiManager.GetEmoji("natureinactive"),
                MonsterClassification.Machina => emojiManager.GetEmoji("machinainactive"),
                MonsterClassification.Spirit => emojiManager.GetEmoji("spiritinactive"),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        else
        {
            return monster.Monster.Classification switch
            {
                MonsterClassification.Rodent => emojiManager.GetEmoji("rodent"),
                MonsterClassification.Nature => emojiManager.GetEmoji("nature"),
                MonsterClassification.Machina => emojiManager.GetEmoji("machina"),
                MonsterClassification.Spirit => emojiManager.GetEmoji("spirit"),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
