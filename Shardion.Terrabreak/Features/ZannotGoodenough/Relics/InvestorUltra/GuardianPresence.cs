using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.InvestorUltra;

public class GuardianPresence : IRelic<RelicState>
{
    public string Name => "Guardian's Presence";
    public string InternalName => "GuardianPresence";
    public string Description => "\"The power of Real compels you!\"";
    public string EffectDescription =>
        "All friendly Spirit-class monsters and Haunting-character monsters have +1 DEF for every friendly Spirit-class monster.";
    public IRelicSeries Series => Registries.RelicSeries.Forward["InvestorUltra"];
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.SpiritMonster, 0.66),
        new(RelicCategory.HauntingCharacter, 0.33),
        new(RelicCategory.Defense, 0.33),
    ];

    public IEnumerable<IBattleDirective>? HookBattleStart(BattleStartInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        foreach (BattleLoadout player in invocation.GetPlayerEnumerator())
        {
            if (!player.GetRelicEnumerator().Contains(thisRelic))
            {
                continue;
            }

            int numberOfSpirits = player
                .GetMonsterEnumerator()
                .Count(monster => monster.Monster.Classification == MonsterClassification.Spirit);
            foreach (BattleMonster monster in player.GetMonsterEnumerator())
            {
                if (monster.Monster.Classification is MonsterClassification.Spirit || monster.Monster.Characteristic == MonsterCharacteristic.Haunting)
                {
                    monster.State.DefenseStaticModifier += numberOfSpirits;
                }
            }
        }

        return null;
    }
}
