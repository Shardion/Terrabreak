using System;
using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Relics.TricksOfTheTrade;

public class CutthroatRanks : IRelic<RelicState>
{
    public string Name => "Cutthroat Ranks";
    public string Description => "I won’t stop till I’m the last one left~";
    public IRelicSeries Series => Registries.RelicSeries.Forward["TricksOfTheTrade"];
    public string EffectDescription =>
        "Every turn, deals an incredible amount of damage to a completely random monster.";
    public IEnumerable<RelicDomainPart> Domain =>
    [
        new(RelicCategory.Attack, 3.0),
    ];

    public IEnumerable<IBattleDirective>? HookTurnStart(TurnStartInvocation invocation, BattleRelic thisRelic, RelicState thisState)
    {
        foreach (BattleLoadout player in invocation.GetPlayerEnumerator())
        {
            if (!player.GetRelicEnumerator().Contains(thisRelic))
            {
                continue;
            }

            BattleLoadout targetPlayer = Random.Shared.Next(2) == 1 ? invocation.Player1 : invocation.Player2;
            // This is probably fine, the battle is over if either side has no monsters standing
            BattleMonster? randomMonster = targetPlayer.GetMonsterEnumerator()
                .Where(monster => !BattleRules.CheckKnockout(monster))
                .Shuffle()
                .FirstOrDefault();
            if (randomMonster is not null)
            {
                return
                [
                    new AttackDirective(
                        new(this, thisRelic, null),
                        new(invocation.Battlefield, player, targetPlayer, randomMonster, randomMonster, 10, 0, 0),
                        $"{targetPlayer.Player.Name}'s {randomMonster.Monster.Name} showed their tail!"
                    ),
                ];
            }
        }

        return null;
    }
}
