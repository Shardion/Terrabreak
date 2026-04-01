using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.InvestorUltra;

public class FocusHarder : IRelic<RelicState>
{
    public string Name => "Focus Harder!";
    public string Description => "Focus, G!";
    public IRelicSeries Series => Registries.RelicSeries.Forward["InvestorUltra"];
    public string EffectDescription => "All friendly monsters will always attack the enemy monster with the lowest HP.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Attack, 2),
    ];

    public TargetSelectionResult? InterceptTargetSelection(TargetSelectionInvocation invocation, BattleRelic thisRelic,
        RelicState thisState)
    {
        if (!invocation.AttackingPlayer.GetRelicEnumerator().Contains(thisRelic))
        {
            return null;
        }

        List<BattleMonster> monsters = invocation.DefendingPlayer.GetMonsterEnumerator()
            .Where(monster => !BattleRules.CheckKnockout(monster)).ToList();
        if (monsters.Count <= 0)
        {
            return new([], null);
        }

        BattleMonster? randomTarget = monsters
            .Aggregate((leastHealthyMonster, currentMonster) =>
                currentMonster.State.CurrentHealth < leastHealthyMonster.State.CurrentHealth
                    ? currentMonster
                    : leastHealthyMonster);
        return new([], randomTarget);
    }
}
