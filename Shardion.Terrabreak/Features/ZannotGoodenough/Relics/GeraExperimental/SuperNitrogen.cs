using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.GeraExperimental;

public class SuperNitrogen : IRelic<RelicState>
{
    public string Name => "Super Nitrogen";
    public string Description => "Your logic is naive.";
    public IRelicSeries Series => Registries.RelicSeries.Forward["GeraExperimental"];
    public string EffectDescription =>
        "The friendly monster in the second loadout slot has 0 ATK, but its Likes ability costs 0 Likes.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Likes, 1.0),
    ];

    public DamageCalculationInvocation? HookDamageCalculation(DamageCalculationInvocation invocation, BattleRelic thisRelic,
        RelicState thisState)
    {
        if (!invocation.AttackingPlayer.GetRelicEnumerator().Contains(thisRelic))
        {
            return null;
        }
        if (invocation.Attacker != invocation.AttackingPlayer.Monster2)
        {
            return null;
        }

        return invocation with { Attack = 0, AttackPercentageModifier = 0, AttackStaticModifier = 0 };
    }

    public LikesCostInvocation<MonsterState>? HookLikesCost(LikesCostInvocation<MonsterState> invocation, BattleRelic thisRelic, RelicState thisState)
    {
        if (!invocation.FriendlyPlayer.GetRelicEnumerator().Contains(thisRelic))
        {
            return null;
        }
        if (invocation.User != invocation.FriendlyPlayer.Monster2)
        {
            return null;
        }

        return invocation with { BaseLikesCost = 0, LikesCostStaticModifier = 0, LikesCostPercentageModifier = 0 };
    }
}
