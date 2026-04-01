using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.InvestorUltra;

public class LineBreakerRotaryCannonModule : IRelic<RelicState>
{
    public string Name => "Line Breaker Rotary Cannon Module";
    public string Description =>
        "A terrifying symbol of engineering progress, this 10-barreled rotary cannon was affixed twice to every Line Breaker, firing at rates exceeding 800 rounds per minute.";
    public IRelicSeries Series => Registries.RelicSeries.Forward["InvestorUltra"];

    public string EffectDescription =>
        "Machina-class monsters and Blasting-character monsters have the cost of their Likes abilities ×0.5.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Likes, 0.5),
        new(RelicCategory.MachinaMonster, 1),
        new(RelicCategory.BlastingCharacter, 1),
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
                .Where(monster => monster.Monster.Classification == MonsterClassification.Machina
                                  || monster.Monster.Characteristic == MonsterCharacteristic.Blasting);
            foreach (BattleMonster monster in applicableMonsters)
            {
                // TODO: Does this work??
                monster.State.LikesCostPercentageModifier = -50;
            }
        }

        return null;
    }
}
