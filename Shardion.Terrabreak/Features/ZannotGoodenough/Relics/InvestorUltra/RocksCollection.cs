using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.InvestorUltra;

public class RocksCollection : IRelic<RelicState>
{
    public string Name => "Rocks Collection";
    public string Description => "What is left to do, collect rocks?";
    public IRelicSeries Series => Registries.RelicSeries.Forward["InvestorUltra"];
    public string EffectDescription =>
        "The friendly monster in the first loadout slot will not be randomly selected as a target, unless it is the only targetable monster.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Defense, 1.0),
    ];

    public TargetSelectionResult? InterceptTargetSelection(TargetSelectionInvocation invocation, BattleRelic thisRelic,
        RelicState thisState)
    {
        if (!invocation.DefendingPlayer.GetRelicEnumerator().Contains(thisRelic))
        {
            return null;
        }

        BattleMonster? randomNonFirstDefendingMonster = invocation.DefendingPlayer.GetMonsterEnumerator()
            .Where(monster => monster != invocation.DefendingPlayer.Monster1
                              && !BattleRules.CheckKnockout(monster))
            .Shuffle()
            .FirstOrDefault();

        if (randomNonFirstDefendingMonster is not null)
        {
            return new([], randomNonFirstDefendingMonster);
        }

        return null;
    }
}
