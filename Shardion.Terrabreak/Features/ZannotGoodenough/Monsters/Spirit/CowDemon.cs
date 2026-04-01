using System.Collections.Generic;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Directives;
using Shardion.Terrabreak.Features.ZannotGoodenough.Gameplay.Invocations;

namespace Shardion.Terrabreak.Features.ZannotGoodenough.Monsters.Spirit;

public class CowDemon : IMonster<MonsterState>
{
    public string Name => "Cow-demon";
    public string InternalName => "CowDemon";
    public string Description => "This one only has a rusty revolver, but higher-ranking officials can carry submachine guns, or even assault rifles.";
    public MonsterClassification Classification => MonsterClassification.Spirit;
    public MonsterCharacteristic Characteristic => MonsterCharacteristic.Blasting;

    public int BaseHealth => 18;
    public int BaseAttack => 4;
    public int BaseDefense => 0;

    public ILikesAbility<MonsterState> LikesAbility { get; } = new LikesAbilitySpinReload();
}

public class LikesAbilitySpinReload : ILikesAbility<MonsterState>
{
    public string Name => "Spin Reload";
    public string Description => "ATK increases by 1, but lose 1 HP.";
    public int BaseLikesCost => 3;

    public IEnumerable<IBattleDirective>? Execute(LikesAbilityInvocation<MonsterState> arguments)
    {
        arguments.User.State.AttackStaticModifier += 1;
        arguments.User.State.CurrentHealth -= 1;

        return null;
    }
}
