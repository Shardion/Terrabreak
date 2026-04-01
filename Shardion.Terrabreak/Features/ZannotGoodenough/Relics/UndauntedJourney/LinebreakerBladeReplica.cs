using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.UndauntedJourney;

public class LinebreakerBladeReplica : IRelic<RelicState>
{
    public string Name => "Linebreaker Blade Replica";

    public string Description =>
        "A mere replica of the legendary weapon, this object lacks the capabilities of the original, but inspires all the same confidence.";
    public IRelicSeries Series => Registries.RelicSeries.Forward["UndauntedJourney"];
    public string EffectDescription => "Friendly Piercing-character monsters have +2 DEF.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Defense, 0.5),
        new(RelicCategory.PiercingCharacter, 0.5),
    ];

    public IEnumerable<IBattleDirective>? HookBattleStart(BattleStartInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        foreach (BattleLoadout player in invocation.GetPlayerEnumerator())
        {
            if (!player.GetRelicEnumerator().Contains(thisRelic))
            {
                continue;
            }

            IEnumerable<BattleMonster> applicableMonsters = player.GetMonsterEnumerator()
                .Where(monster => monster.Monster.Characteristic == MonsterCharacteristic.Piercing);
            foreach (BattleMonster monster in applicableMonsters)
            {
                monster.State.DefenseStaticModifier += 2;
            }
        }

        return null;
    }
}
