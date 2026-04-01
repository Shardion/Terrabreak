using System.Collections.Generic;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.InvestorCore;

public class ShadowWizardMoney : IRelic<RelicState>
{
    public string Name => "Shadow Wizard Money";
    public IRelicSeries Series => Registries.RelicSeries.Forward["InvestorCore"];
    public string Description => "We love casting spells!!";
    public string EffectDescription =>
        "Spirit-class monsters generate 4 Likes when attacking, or 8 Likes when attacking a Spirit-class or Machina-class monster.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.SpiritMonster, 0.5),
        new(RelicCategory.Likes, 0.5),
    ];

    public IEnumerable<IBattleDirective>? HookAttack(AttackInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        if (invocation.Attacker.Monster.Classification is MonsterClassification.Spirit)
        {
            int likes = 4;
            if (invocation.Defender.Monster.Classification is MonsterClassification.Spirit
                or MonsterClassification.Machina)
            {
                likes += 4;
            }

            return
            [
                new GrantLikesDirective(
                    new(thisRelic, thisRelic, invocation.Attacker),
                    new(invocation.Battlefield, invocation.AttackingPlayer, likes)
                )
            ];
        }

        return null;
    }
}
