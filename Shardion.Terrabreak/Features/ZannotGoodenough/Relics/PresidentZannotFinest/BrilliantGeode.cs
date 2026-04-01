using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.PresidentZannotFinest;

public class BrilliantGeode : IRelic<RelicState>
{
    public string Name => "'Brilliant Geode'";
    public string Description => "Reflects our past.";
    public IRelicSeries Series => Registries.RelicSeries.Forward["PresidentZannotFinest"];
    public string EffectDescription => "The friendly monster in the third loadout slot gains +2 DEF.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Defense, 1),
    ];

    public IEnumerable<IBattleDirective>? HookBattleStart(BattleStartInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        foreach (BattleLoadout player in invocation.GetPlayerEnumerator())
        {
            if (!player.GetRelicEnumerator().Contains(thisRelic))
            {
                continue;
            }

            player.Monster3?.State.DefenseStaticModifier += 2;
        }

        return null;
    }
}
