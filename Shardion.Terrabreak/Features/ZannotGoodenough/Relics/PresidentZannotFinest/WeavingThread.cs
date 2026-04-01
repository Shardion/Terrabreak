using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.PresidentZannotFinest;

public class WeavingThread : IRelic<RelicState>
{
    public string Name => "'Weaving Thread'";
    public string Description => "Binds us all.";
    public IRelicSeries Series => Registries.RelicSeries.Forward["PresidentZannotFinest"];
    public string EffectDescription => "The friendly monster in the first loadout slot gains +3 ATK.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Attack, 1),
    ];

    public IEnumerable<IBattleDirective>? HookBattleStart(BattleStartInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        foreach (BattleLoadout player in invocation.GetPlayerEnumerator())
        {
            if (!player.GetRelicEnumerator().Contains(thisRelic))
            {
                continue;
            }

            player.Monster1?.State.AttackStaticModifier += 3;
        }

        return null;
    }
}
