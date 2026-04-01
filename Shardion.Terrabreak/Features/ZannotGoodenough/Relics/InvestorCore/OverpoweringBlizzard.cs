using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.InvestorCore;

public class OverpoweringBlizzard : IRelic<RelicState>
{
    public string Name => "Overpowering Blizzard";
    public string Description => "Surely the work of the once-feared Blizzard Pig!";
    public IRelicSeries Series => Registries.RelicSeries.Forward["InvestorCore"];
    public string EffectDescription => "Friendly Haunting-character and Blasting-character monsters gain +2 ATK.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.HauntingCharacter, 0.5),
        new(RelicCategory.BlastingCharacter, 0.5),
        new(RelicCategory.Attack, 0.5),
    ];

    public IEnumerable<IBattleDirective>? HookBattleStart(BattleStartInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        foreach (BattleLoadout player in invocation.GetPlayerEnumerator())
        {
            if (!player.GetRelicEnumerator().Contains(thisRelic))
            {
                continue;
            }
            foreach (BattleMonster monster in player.GetMonsterEnumerator())
            {
                if (monster.Monster.Characteristic is MonsterCharacteristic.Haunting or MonsterCharacteristic.Blasting)
                {
                    monster.State.AttackStaticModifier += 2;
                }
            }
        }
        return null;
    }
}
