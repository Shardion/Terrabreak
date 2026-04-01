using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.PresidentZannotFinest;

public class StopTheRangers : IRelic<RelicState>
{
    public string Name => "Stop The Rangers!";
    public string InternalName => "StopTheRangers";
    public string Description => "\"And the plant told me that I was overreacting!\"";
    public IRelicSeries Series => Registries.RelicSeries.Forward["PresidentZannotFinest"];
    public string EffectDescription => "Friendly monsters have +2 ATK and -2 DEF.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Attack, 1.0),
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
                monster.State.AttackStaticModifier += 2;
                monster.State.DefenseStaticModifier -= 2;
            }
        }

        return null;
    }
}
