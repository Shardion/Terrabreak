using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.PresidentZannotFinest;

public class CelestialDust: IRelic<RelicState>
{
    public string Name => "'Celestial Dust'";
    public string Description => "Shows us the way.";
    public IRelicSeries Series => Registries.RelicSeries.Forward["PresidentZannotFinest"];
    public string EffectDescription => "Attacks from the friendly monster in the second loadout slot grant an extra like.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Attack, 1),
    ];

    public IEnumerable<IBattleDirective>? HookAttack(AttackInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        if (!invocation.AttackingPlayer.GetRelicEnumerator().Contains(thisRelic))
        {
            return null;
        }
        if (invocation.AttackingPlayer.Monster2 != invocation.Attacker)
        {
            return null;
        }

        return
        [
            new GrantLikesDirective(
                new(this, thisRelic, invocation.Attacker),
                new(invocation.Battlefield, invocation.AttackingPlayer, 1)
            ),
        ];
    }
}
