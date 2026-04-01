using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.UndauntedJourney;

public class GotDemons : IRelic<RelicState>
{
    public string Name => "Got Demons?";
    public string InternalName => "GotDemons";
    public string Description => "Which ones, you ask? Please refer to the chart.";
    public IRelicSeries Series => Registries.RelicSeries.Forward["UndauntedJourney"];
    public string EffectDescription => "Friendly Spirit-class monsters have a 50% chance to dodge attacks.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.SpiritMonster, 1.0),
        new(RelicCategory.Defense, 0.5),
    ];

    public IEnumerable<IBattleDirective>? HookBattleStart(BattleStartInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        foreach (BattleLoadout player in invocation.GetPlayerEnumerator())
        {
            if (!player.GetRelicEnumerator().Contains(thisRelic))
            {
                continue;
            }

            IEnumerable<BattleMonster> applicableMonsters = player.GetMonsterEnumerator()
                .Where(monster => monster.Monster.Classification == MonsterClassification.Spirit);
            foreach (BattleMonster monster in applicableMonsters)
            {
                monster.State.DodgeChance += 0.25;
            }
        }

        return null;
    }
}
