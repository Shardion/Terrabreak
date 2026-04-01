using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.TricksOfTheTrade;

public class SpokenVeil : IRelic<RelicState>
{
    public string Name => "Spoken Veil";
    public string Description => "One of the spokes is missing.";
    public IRelicSeries Series => Registries.RelicSeries.Forward["TricksOfTheTrade"];
    public string EffectDescription =>
        "Grants a 33% dodge chance to all friendly monsters.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Defense, 3.0),
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
                monster.State.DodgeChance += 0.33;
            }
        }

        return null;
    }
}
