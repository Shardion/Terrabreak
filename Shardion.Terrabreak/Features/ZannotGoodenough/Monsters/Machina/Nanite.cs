using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Machina;

public class Nanite : IMonster<MonsterState>
{
    public string Name => "BGN-7 \"Nanite\"";
    public string InternalName => "Nanite";
    public string Description => "Nanites are often deployed to battlefields to repair other machines in emergencies.";
    public MonsterClassification Classification => MonsterClassification.Machina;
    public MonsterCharacteristic Characteristic => MonsterCharacteristic.Blunt;

    public int BaseHealth => 25;
    public int BaseAttack => 1;
    public int BaseDefense => 3;

    public ILikesAbility<MonsterState> LikesAbility { get; } = new LikesAbilityRepair();
}

public class LikesAbilityRepair : ILikesAbility<MonsterState>
{
    public string Name => "Repair";
    public string Description => "Heals a random friendly monster for 4 HP.";
    public int BaseLikesCost => 6;

    public IEnumerable<IBattleDirective>? Execute(LikesAbilityInvocation<MonsterState> arguments)
    {
        BattleMonster targetMonster;
        BattleMonster? randomFriendlyMachinaMonster = arguments.FriendlyPlayer.GetMonsterEnumerator()
            .Where(monster => monster.Monster.Classification == MonsterClassification.Machina
                              && monster.State != arguments.UserState
                              && monster.State.CurrentHealth < monster.State.MaxHealth
                              && !BattleRules.CheckKnockout(monster))
            .Shuffle()
            .FirstOrDefault();
        BattleMonster? randomFriendlyMonster = arguments.FriendlyPlayer.GetMonsterEnumerator()
            .Where(monster => monster.State != arguments.UserState
                              && monster.State.CurrentHealth < monster.State.MaxHealth
                              && !BattleRules.CheckKnockout(monster))
            .Shuffle()
            .FirstOrDefault();
        if (randomFriendlyMachinaMonster is not null)
        {
            targetMonster = randomFriendlyMachinaMonster;
        }
        else if (randomFriendlyMonster is not null)
        {
            targetMonster = randomFriendlyMonster;
        }
        else
        {
            return null;
        }

        return
        [
            new HealDirective(
                new(arguments.User, null, arguments.User),
                new(arguments.Battlefield, arguments.FriendlyPlayer, arguments.EnemyPlayer,
                    arguments.User, targetMonster, 4, 0, 0)
            )
        ];
    }
}
