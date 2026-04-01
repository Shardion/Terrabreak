using System.Collections.Generic;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;
using Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Nature;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Spirit;

public class BlueEyesWhiteDragon : IMonster<MonsterState>
{
    public string Name => "Blue-eyes White Dragon";
    public string InternalName => "BlueEyesWhiteDragon";
    public string Description => "...What?";
    public MonsterClassification Classification => MonsterClassification.Spirit;
    public MonsterCharacteristic Characteristic => MonsterCharacteristic.Blasting;

    public int BaseHealth => 2500;
    public int BaseAttack => 3000;
    public int BaseDefense => 2500;

    public bool Hidden => true;

    public ILikesAbility<MonsterState> LikesAbility => new LikesAbilityInstantWin();
}

public class LikesAbilityInstantWin : ILikesAbility<MonsterState>
{
    public string Name => "Instant Win";
    public string Description => "Win immediately.";
    public int BaseLikesCost => 0;

    public IEnumerable<IBattleDirective>? Execute(LikesAbilityInvocation<MonsterState> arguments)
    {
        List<AttackDirective> hits = [];
        foreach (BattleMonster monster in arguments.EnemyPlayer.GetMonsterEnumerator())
        {
            monster.State.CurrentHealth = 0;
            hits.Add(new(
                new(arguments.User, null, arguments.User),
                new(arguments.Battlefield, arguments.FriendlyPlayer, arguments.EnemyPlayer, arguments.User, monster, 2241, 0, 0)
            ));
        }
        return hits;
    }
}
