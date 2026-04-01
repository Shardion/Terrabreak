using System.Collections.Generic;
using System.Linq;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Machina;

public class LineBreaker : IMonster<MonsterState>
{
    public string Name => "BGN-4 \"Line Breaker\"";
    public string InternalName => "LineBreaker";
    public string Description =>
        "Best compared to quadrupedal siege weapons, the mere sight of a Line Breaker strikes fear into even the most hardened fighters.";

    public int BaseHealth => 18;
    public int BaseAttack => 6;
    public int BaseDefense => 0;
    public MonsterClassification Classification => MonsterClassification.Machina;
    public MonsterCharacteristic Characteristic => MonsterCharacteristic.Blasting;
    public ILikesAbility<MonsterState> LikesAbility => new TrampleLikesAbility();
}

public class TrampleLikesAbility : ILikesAbility<MonsterState>
{
    public string Name => "Trample";
    public string Description => "Attack all enemy monsters with less than 7 HP remaining.";
    public int BaseLikesCost => 11;

    public IEnumerable<IBattleDirective>? Execute(LikesAbilityInvocation<MonsterState> arguments)
    {
        List<BasicAttackDirective> attacks = [];
        IEnumerable<BattleMonster> enemyMonsters = arguments.EnemyPlayer.GetMonsterEnumerator()
            .Where(monster => monster.State.CurrentHealth <= 7);
        foreach (BattleMonster monster in enemyMonsters)
        {
            attacks.Add(new(
                new(arguments.User, null, arguments.User),
                new(arguments.Battlefield, arguments.FriendlyPlayer, arguments.EnemyPlayer, arguments.User, monster)
            ));
        }
        return attacks;
    }
}
