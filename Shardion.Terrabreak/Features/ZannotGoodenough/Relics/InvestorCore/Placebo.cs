using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.InvestorCore;

public class Placebo : IRelic<RelicState>
{
    public string Name => "Placebo";
    public string Description => "You probably know how these work by now.";
    public string EffectDescription =>
        "Friendly Rodent-class and Nature-class monsters gain 1 HP for every friendly Rodent-class monster in play.";
    public IRelicSeries Series => Registries.RelicSeries.Forward["InvestorCore"];
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.RodentMonster, 0.66),
        new(RelicCategory.NatureMonster, 0.33),
    ];

    public IEnumerable<IBattleDirective>? HookBattleStart(BattleStartInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        foreach (BattleLoadout player in invocation.GetPlayerEnumerator())
        {
            if (!player.GetRelicEnumerator().Contains(thisRelic))
            {
                continue;
            }

            int numberOfRodents = player
                .GetMonsterEnumerator()
                .Count(monster => monster.Monster.Classification == MonsterClassification.Rodent);
            foreach (BattleMonster monster in player.GetMonsterEnumerator())
            {
                if (monster.Monster.Classification is MonsterClassification.Rodent or MonsterClassification.Spirit)
                {
                    monster.State.CurrentHealth += numberOfRodents;
                    monster.State.MaxHealth += numberOfRodents;
                }
            }
        }

        return null;
    }
}
