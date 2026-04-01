using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.InvestorCore;

public class TheZombies : IRelic<RelicState>
{
    public string Name => "The Zombies";
    public string Description => "...an all-out attack on your houze.";
    public IRelicSeries Series => Registries.RelicSeries.Forward["InvestorCore"];
    public string EffectDescription => "Friendly monsters prioritise attacking enemy Nature-class monsters.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Targeting, 1.0),
    ];
    public IEnumerable<RelicCategory> Conflicts => [RelicCategory.Targeting];

    public TargetSelectionResult? InterceptTargetSelection(TargetSelectionInvocation invocation, BattleRelic thisRelic,
        RelicState thisState)
    {
        if (!invocation.AttackingPlayer.GetRelicEnumerator().Contains(thisRelic))
        {
            return null;
        }

        BattleMonster? enemyNatureMonster = invocation.DefendingPlayer.GetMonsterEnumerator()
            .Shuffle()
            .FirstOrDefault(monster => monster.Monster.Classification == MonsterClassification.Nature
                                       && !BattleRules.CheckKnockout(monster));
        if (enemyNatureMonster is not null)
        {
            return new([], enemyNatureMonster);
        }

        return null;
    }
}
