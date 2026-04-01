using System;
using System.Collections.Generic;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.UndauntedJourney;

public class IntensiveNuggetCare : IRelic<RelicState>
{
    public string Name => "Intensive Nugget Care";
    public string Description => "Fail!";
    public IRelicSeries Series => Registries.RelicSeries.Forward["UndauntedJourney"];
    public string EffectDescription =>
        "Healing applied to all Machina-class monsters in play is instead inflicted as damage.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Attack, 1.0),
    ];

    public IEnumerable<IBattleDirective>? InterceptHeal(HealInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        if (invocation.Receiver.Monster.Classification == MonsterClassification.Machina)
        {
            return
            [
                new AttackDirective(
                    new(this,
                        thisRelic,
                        invocation.Healer
                    ),
                    new(invocation.Battlefield,
                        invocation.HealingPlayer,
                        invocation.HealingPlayer,
                        invocation.Healer,
                        invocation.Receiver, invocation.BaseHealing,
                        invocation.HealingPercentageBoost,
                        invocation.HealingStaticBoost
                    )
                )
            ];
        }

        return null;
    }
}
