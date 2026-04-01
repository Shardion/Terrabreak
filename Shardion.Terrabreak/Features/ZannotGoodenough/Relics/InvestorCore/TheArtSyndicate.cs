using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.InvestorCore;

public class TheArtSyndicate : IRelic<RelicState>
{
    public string Name => "The Art Syndicate";
    public string Description => "Enchart, are you watching?";
    public IRelicSeries Series => Registries.RelicSeries.Forward["InvestorCore"];
    public string EffectDescription => "Friendly monsters prioritise attacking enemy Machina-class monsters.";
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

        BattleMonster? enemyMachinaMonster = invocation.DefendingPlayer.GetMonsterEnumerator()
            .Shuffle()
            .FirstOrDefault(monster => monster.Monster.Classification == MonsterClassification.Machina
                                       && !BattleRules.CheckKnockout(monster));
        if (enemyMachinaMonster is not null)
        {
            return new([], enemyMachinaMonster);
        }

        return null;
    }
}
