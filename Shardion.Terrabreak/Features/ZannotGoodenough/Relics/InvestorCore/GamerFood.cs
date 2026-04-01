using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.InvestorCore;

public class GamerFood : IRelic<RelicState>
{
    public string Name => "Gamer Food";
    public string Description => "The most favored treat of The Gamer.";
    public string EffectDescription => "Friendly Rodent-class monsters have +2 ATK.";
    public IRelicSeries Series => Registries.RelicSeries.Forward["InvestorCore"];
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Attack, 1.0),
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
                .Where(monster => monster.Monster.Classification == MonsterClassification.Rodent);
            foreach (BattleMonster monster in applicableMonsters)
            {
                monster.State.AttackStaticModifier += 2;
            }
        }

        return null;
    }
}
