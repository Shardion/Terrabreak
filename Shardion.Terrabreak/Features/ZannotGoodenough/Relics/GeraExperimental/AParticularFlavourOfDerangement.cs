using System;
using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.GeraExperimental;

public class AParticularFlavourOfDerangement : IRelic<RelicState>
{
    public string Name => "A Particular Flavour Of Derangement";
    public string Description => "I'm bounded by these shackles of shame...";
    public IRelicSeries Series => Registries.RelicSeries.Forward["GeraExperimental"];
    public string EffectDescription =>
        "The friendly monster in the third loadout slot has its stats customised every turn.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
    ];

    public IEnumerable<IBattleDirective>? HookTurnStart(TurnStartInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        foreach (BattleLoadout player in invocation.GetPlayerEnumerator())
        {
            if (!player.GetRelicEnumerator().Contains(thisRelic))
            {
                continue;
            }

            player.Monster3?.State.DefenseStaticModifier = Random.Shared.Next(0, 5);
            player.Monster3?.State.AttackStaticModifier = Random.Shared.Next(1, 8);
        }

        return null;
    }
}
