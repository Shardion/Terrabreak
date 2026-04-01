using System.Collections.Generic;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Machina;

public class Silkseeker : IMonster<MonsterState>
{
    public string Name => "\"Silkseeker\"";
    public string Description =>
        "These machines were built with the sole purpose of defending a facility from advanced threats, and can communicate wirelessly to perform strategic maneuvers.";

    public int BaseHealth => 12;
    public int BaseAttack => 8;
    public int BaseDefense => 0;
    public MonsterClassification Classification => MonsterClassification.Machina;
    public MonsterCharacteristic Characteristic => MonsterCharacteristic.Blasting;
    public ILikesAbility<MonsterState> LikesAbility => new NeurolaserLikesAbility();
}

public class NeurolaserLikesAbility : ILikesAbility<MonsterState>
{
    public string Name => "Neurolaser";
    public string Description => "Use an attack that hits all enemy monsters.";
    public int BaseLikesCost => 8;

    public IEnumerable<IBattleDirective>? Execute(LikesAbilityInvocation<MonsterState> arguments)
    {
        List<BasicAttackDirective> attacks = [];
        IEnumerable<BattleMonster> enemyMonsters = arguments.EnemyPlayer.GetMonsterEnumerator();
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
