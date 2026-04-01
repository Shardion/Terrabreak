using System.Collections.Generic;
using Serilog;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.InvestorUltra;

public class BlurayBlastingBomb : IRelic<RelicState>
{
    public string Name => "Blu-ray Blasting Bomb";
    public string InternalName => "BlurayBlastingBomb";
    public string Description => "\"Everything's on sale!\"";
    public IRelicSeries Series => Registries.RelicSeries.Forward["InvestorUltra"];
    public string EffectDescription => "All Machina-class monsters in play have 2 ATK.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Defense, 2.0),
    ];

    public DamageCalculationInvocation? HookDamageCalculation(DamageCalculationInvocation invocation, BattleRelic thisRelic,
        RelicState thisState)
    {
        bool relevant = invocation.Attacker is { Monster.Classification: MonsterClassification.Machina };
        Log.Debug("Blu-ray Blasting Bomb relevant for {monster}: {relevant}", invocation.Attacker.Monster.Name, relevant);
        if (invocation.Attacker is { Monster.Classification: MonsterClassification.Machina })
        {
            return invocation with { Attack = 2, AttackStaticModifier = 0, AttackPercentageModifier = 0 };
        }

        return null;
    }
}
