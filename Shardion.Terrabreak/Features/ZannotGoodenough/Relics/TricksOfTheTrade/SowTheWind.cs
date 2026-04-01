using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.TricksOfTheTrade;

public class SowTheWind : IRelic<RelicState>
{
    public string Name => "Sow the Wind";
    public string InternalName => "SowTheWind";
    public string Description => "\"...You won't leave this place alive!\"";
    public IRelicSeries Series => Registries.RelicSeries.Forward["TricksOfTheTrade"];
    public string EffectDescription => "When only one friendly monster stands, its stats are doubled.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Attack, 2.0),
        new(RelicCategory.Defense, 2.0),
    ];

    public DamageCalculationInvocation? HookDamageCalculation(DamageCalculationInvocation invocation, BattleRelic thisRelic,
        RelicState thisState)
    {
        int attack = invocation.Attack;
        int defense = invocation.Defense;
        if (invocation.AttackingPlayer.GetRelicEnumerator().Contains(thisRelic))
        {
            if (invocation.AttackingPlayer.GetMonsterEnumerator().Count() <= 1)
            {
                attack = invocation.Attack * 2;
            }
        }
        if (invocation.DefendingPlayer.GetRelicEnumerator().Contains(thisRelic))
        {
            if (invocation.DefendingPlayer.GetMonsterEnumerator().Count() <= 1)
            {
                defense = invocation.Defense * 2;
            }
        }

        if (attack != invocation.Attack || defense != invocation.Defense)
        {
            return invocation with { Attack = attack, Defense = defense };
        }

        return null;
    }
}
