using System;
using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.TricksOfTheTrade;

public class BlessedRelic : IRelic<RelicState>
{
    public string Name => "Blessed Relic";
    public string Description => "\"...As I, too, am only a player in this game.\"";
    public IRelicSeries Series => Registries.RelicSeries.Forward["TricksOfTheTrade"];
    public string EffectDescription => "All permanent stat changes are multiplied by ×1.5, including other multipliers.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Attack, 2.0),
        new(RelicCategory.Defense, 2.0),
    ];

    public IEnumerable<IBattleDirective>? HookBattleStart(BattleStartInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        foreach (BattleLoadout player in invocation.GetPlayerEnumerator())
        {
            if (!player.GetRelicEnumerator().Contains(thisRelic))
            {
                continue;
            }

            foreach (BattleMonster monster in player.GetMonsterEnumerator())
            {
                monster.State.AttackStaticModifier = Convert.ToInt32(Math.Round(monster.State.AttackStaticModifier * 1.5, MidpointRounding.ToZero));
                monster.State.AttackPercentageModifier = Convert.ToInt32(Math.Round(monster.State.AttackPercentageModifier * 1.5, MidpointRounding.ToZero));
                monster.State.DefenseStaticModifier = Convert.ToInt32(Math.Round(monster.State.DefenseStaticModifier * 1.5, MidpointRounding.ToZero));
                monster.State.DefensePercentageModifier = Convert.ToInt32(Math.Round(monster.State.DefensePercentageModifier * 1.5, MidpointRounding.ToZero));
            }
        }

        return null;
    }
}
