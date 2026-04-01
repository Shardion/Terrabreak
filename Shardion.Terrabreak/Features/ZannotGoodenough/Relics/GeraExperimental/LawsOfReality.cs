using System;
using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.GeraExperimental;

public class LawsOfReality : IRelic<RelicState>
{
    public string Name => "Laws of Reality";
    public string InternalName => "LawsOfReality";
    public string Description => "The forces of...";
    public IRelicSeries Series => Registries.RelicSeries.Forward["GeraExperimental"];
    public string EffectDescription =>
        "Every point of damage taken by friendly monsters removes a Like. Every point of damage dealt by friendly monsters grants a Like.";

    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Attack, 0.5),
        new(RelicCategory.Likes, 0.5),
    ];

    public IEnumerable<IBattleDirective>? HookAttack(AttackInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        int damage = Convert.ToInt32(Math.Round(
            (invocation.BaseFinalDamage + invocation.FinalDamageStaticBoost) *
            ((invocation.FinalDamagePercentageBoost + 100) * 0.01),
            MidpointRounding.AwayFromZero));

        List<GrantLikesDirective> directives = [];
        if (invocation.AttackingPlayer.GetRelicEnumerator().Contains(thisRelic))
        {
            directives.Add(new(new(this, thisRelic, invocation.Attacker), new(invocation.Battlefield, invocation.AttackingPlayer, damage)));
        }
        if (invocation.DefendingPlayer.GetRelicEnumerator().Contains(thisRelic))
        {
            directives.Add(new(new(this, thisRelic, invocation.Attacker), new(invocation.Battlefield, invocation.DefendingPlayer, -damage)));
        }

        return directives;
    }
}
