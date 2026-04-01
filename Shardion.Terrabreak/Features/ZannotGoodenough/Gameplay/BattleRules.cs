using System;
using System.Linq;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;

public static class BattleRules
{
    public static bool CheckKnockout(BattleMonster monster)
    {
        return monster.State.CurrentHealth <= 0 && monster.Monster.BaseHealth > 0;
    }

    public static bool CheckPlayerLoss(BattleLoadout player)
    {
        return player.GetMonsterEnumerator()
            .All(CheckKnockout);
    }
}
